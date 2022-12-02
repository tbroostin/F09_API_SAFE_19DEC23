// Copyright 2017-2022 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Exceptions;
using slf4net;
using Ellucian.Data.Colleague.DataContracts;
using System.Collections.ObjectModel;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.Common.Utility;
using Ellucian.Web.Http.Configuration;
using Ellucian.Dmi.Runtime;
using Ellucian.Colleague.Configuration;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Base.Services;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Data.Base.Repositories
{
    [RegisterType]
    public class ConfigurationRepository : BaseColleagueRepository, IConfigurationRepository
    {
        private readonly string colleagueTimeZone;
        private readonly string orgIndicator;
        private IColleagueTransactionInvoker anonymousTransactionInvoker;
        char _VM = Convert.ToChar(DynamicArray.VM);
        char _SM = Convert.ToChar(DynamicArray.SM);
        char _TM = Convert.ToChar(DynamicArray.TM);
        char _XM = Convert.ToChar(250);

        /// <summary>
        /// Contains a Tuple where Item1 is a bool set to true if any fields are denied or secured, 
        /// Item2 is a list of DeniedAccess Fields and Item3 is a list of Restricted fields.
        /// </summary>
        public Tuple<bool, List<string>, List<string>> SecureDataDefinition { get; set; }

        /// <summary>
        /// Get the Denied and Secured Data Properties from the CTX call.
        /// </summary>
        /// <returns>Returns a Tuple with the secure flag and list of denied data fields and list of secure data fields.</returns>
        public Tuple<bool, List<string>, List<string>> GetSecureDataDefinition()
        {
            if (SecureDataDefinition == null)
            {
                SecureDataDefinition = new Tuple<bool, List<string>, List<string>>(false, new List<string>(), new List<string>());
            }
            return SecureDataDefinition;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationRepository"/> class.
        /// </summary>
        /// <param name="cacheProvider">The cache provider.</param>
        /// <param name="transactionFactory">The transaction factory.</param>
        /// <param name="logger">The logger.</param>
        public ConfigurationRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory,
            ApiSettings settings, ILogger logger, ColleagueSettings colleagueSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using level 1 cache time out value for data that rarely changes.
            CacheTimeout = Level1CacheTimeoutValue;
            orgIndicator = "ORG";
            colleagueTimeZone = settings.ColleagueTimeZone;

            // transactionInvoker will only be non-null when a user is logged in
            // If this is being requested anonymously, create a transaction invoker 
            // without any user context.
            anonymousTransactionInvoker = transactionInvoker ?? new ColleagueTransactionInvoker(null, null, logger, colleagueSettings.DmiSettings);
        }

        #region Public Methods

        /// <summary>
        /// Gets all of the EthosDataPrivacy settings stored on ETHOS.SECURITY accessed on form EDPS
        /// </summary>
        /// <param name="bypassCache">bool to determine if cache should be bypassed</param>
        /// <returns>List of Ellucian.Colleague.Domain.Base.Entities.EthosSecurity</returns>
        public async Task<IEnumerable<Domain.Base.Entities.EthosSecurity>> GetEthosDataPrivacyConfiguration(bool bypassCache)
        {
            const string ethosDataPrivacyCacheKey = "AllEthosDataPrivacySettings";

            if (bypassCache && ContainsKey(BuildFullCacheKey(ethosDataPrivacyCacheKey)))
            {
                ClearCache(new List<string> { ethosDataPrivacyCacheKey });
            }

            var ethosDataPrivacySettingsList = await GetOrAddToCacheAsync<List<Domain.Base.Entities.EthosSecurity>>(ethosDataPrivacyCacheKey,
                async () =>
                {
                    var ethosDpSettings = await DataReader.BulkReadRecordAsync<DataContracts.EthosSecurity>("ETHOS.SECURITY", "");
                    return (await BuildEthosSecurityList(ethosDpSettings)).ToList();
                }
            , CacheTimeout);
            return ethosDataPrivacySettingsList;
        }

        /// <summary>
        /// Checks if the user making the API call is the EMA user based on the user settings on the EMA configuration
        /// </summary>
        /// <param name="userName">userName to check</param>
        /// <param name="bypassCache">bool to determine if the cache should be bypassed</param>
        /// <returns>True if EMA user, false if not</returns>
        public async Task<bool> IsThisTheEmaUser(string userName, bool bypassCache)
        {
            const string hubCincRecord = "HubRecordFromCINC";
            if (bypassCache && ContainsKey(BuildFullCacheKey(hubCincRecord)))
            {
                ClearCache(new List<string> { hubCincRecord });
            }

            var hubRecordToCheck =
                await GetOrAddToCacheAsync<DataContracts.CdmIntegration>(hubCincRecord,
                async () => (await DataReader.BulkReadRecordAsync<DataContracts.CdmIntegration>("CDM.INTEGRATION", "WITH CDM.INTEGRATION.ID EQ 'HUB'")).FirstOrDefault(), CacheTimeout);

            return hubRecordToCheck != null && hubRecordToCheck.CintApiUsername.Equals(userName, StringComparison.OrdinalIgnoreCase);
        }

        private IEnumerable<EdmExtVersions> _ethosExtensiblitySettingsListByResource = null;
        /// <summary>
        /// Gets all of the Ethos Extensiblity settings stored on EDM.EXT.VERSIONS
        /// </summary>
        /// <param name="bypassCache">bool to determine if cache should be bypassed</param>
        /// <returns>List of DataContracts.EdmExtVersions</returns>
        private async Task<IEnumerable<EdmExtVersions>> GetEthosExtensibilityConfiguration(string resource, bool bypassCache = false)
        {
            if (_ethosExtensiblitySettingsListByResource == null)
            {
                string ethosExtensiblityCacheKey = "AllEthosExtensibiltySettingsByResource" + resource;

                if (bypassCache && ContainsKey(BuildFullCacheKey(ethosExtensiblityCacheKey)))
                {
                    ClearCache(new List<string> { ethosExtensiblityCacheKey });
                }

                string criteria = string.Format("WITH EDMV.RESOURCE.NAME = '{0}'", resource);
                var ethosExtensiblitySettingsList =
                    await GetOrAddToCacheAsync<List<EdmExtVersions>>(ethosExtensiblityCacheKey,
                        async () => (await DataReader.BulkReadRecordAsync<EdmExtVersions>("EDM.EXT.VERSIONS", criteria)).ToList(), CacheTimeout);

                _ethosExtensiblitySettingsListByResource = ethosExtensiblitySettingsList;
            }
            return _ethosExtensiblitySettingsListByResource;
        }

        private IEnumerable<EdmCodeHooks> _ethosExtensibleCodeHooks = null;
        /// <summary>
        /// Gets all of the Ethos Extensiblity code hook settings stored on EDM.CODE.HOOKS
        /// </summary>
        /// <param name="bypassCache">bool to determine if cache should be bypassed</param>
        /// <returns>List of DataContracts.EdmCodeHooks</returns>
        public async Task<IEnumerable<EdmCodeHooks>> GetEthosExtensibilityCodeHooks(bool bypassCache = false)
        {
            if (_ethosExtensibleCodeHooks == null)
            {
                const string ethosExtensiblityCacheKey = "AllEthosExtensibiltyCodeHooks";

                if (bypassCache && ContainsKey(BuildFullCacheKey(ethosExtensiblityCacheKey)))
                {
                    ClearCache(new List<string> { ethosExtensiblityCacheKey });
                }

                var ethosExtensiblityCodeHooks = new List<EdmCodeHooks>();
                try
                {
                    ethosExtensiblityCodeHooks =
                        await GetOrAddToCacheAsync<List<EdmCodeHooks>>(ethosExtensiblityCacheKey,
                            async () => (await DataReader.BulkReadRecordAsync<EdmCodeHooks>("EDM.CODE.HOOKS", "")).ToList(), CacheTimeout);
                }
                catch (Exception ex)
                {
                    // If we are missing code hooks table, then do not throw an exception.
                    logger.Error(ex.Message, "Missing code hooks table.");
                }

                _ethosExtensibleCodeHooks = ethosExtensiblityCodeHooks;
            }
            return _ethosExtensibleCodeHooks;
        }


        /// <summary>
        /// Gets all of the Ethos Extensiblity settings stored on EDM.EXT.VERSIONS
        /// </summary>
        /// <param name="bypassCache">bool to determine if cache should be bypassed</param>
        /// <returns>List of Domain.Base.Entities.EthosExtensibleData</returns>
        public async Task<IEnumerable<Domain.Base.Entities.EthosExtensibleData>> GetEthosExtensibilityConfigurationEntities(bool customOnly = true, bool bypassCache = false)
        {
            var retVal = new List<Domain.Base.Entities.EthosExtensibleData>();
            const string ethosExtensiblityCacheKey = "AllEthosExtensibiltySettingsAndData";

            if (bypassCache && ContainsKey(BuildFullCacheKey(ethosExtensiblityCacheKey)))
            {
                ClearCache(new List<string> { ethosExtensiblityCacheKey + "VERSIONS" ,
                                              ethosExtensiblityCacheKey + "DEPRECATED",
                                              ethosExtensiblityCacheKey + "EXTENSIONS"});
            }

            List<string> edmeVersions = new List<string>();
            ICollection<EdmExtensions> extensions = null;
            if (customOnly)
               extensions = await DataReader.BulkReadRecordAsync<EdmExtensions>("EDM.EXTENSIONS", "WITH EDME.PRIMARY.GUID.SOURCE NE '' OR WITH EDME.PRIMARY.KEY NE ''");
            else
                extensions = await DataReader.BulkReadRecordAsync<EdmExtensions>("EDM.EXTENSIONS", "");

            foreach (var ext in extensions)
            {
                edmeVersions.AddRange(ext.EdmeVersions);
            }         
          
            var ethosExtensiblitySettingsList =
                await GetOrAddToCacheAsync<List<EdmExtVersions>>(ethosExtensiblityCacheKey + "VERSIONS",
                    async () => (await DataReader.BulkReadRecordAsync<EdmExtVersions>("EDM.EXT.VERSIONS", edmeVersions.ToArray(),
                        false)).ToList(), CacheTimeout);


            /*  var ethosExtensiblitySettingsList =
                await GetOrAddToCacheAsync<List<EdmExtVersions>>(ethosExtensiblityCacheKey + "VERSIONS",
                    async () => (await DataReader.BulkReadRecordAsync<EdmExtVersions>("EDM.EXT.VERSIONS", edmeVersions.Any() ? edmeVersions.ToArray() : null,
                        false)).ToList(), CacheTimeout); 
             */
            var deprecatedData =
                 await GetOrAddToCacheAsync<List<EdmDepNotices>>(ethosExtensiblityCacheKey + "DEPRECATED",
                    async () => (await DataReader.BulkReadRecordAsync<EdmDepNotices>("EDM.DEP.NOTICES", "")).ToList(), CacheTimeout);

            var extensionsData =
                await GetOrAddToCacheAsync<List<EdmExtensions>>(ethosExtensiblityCacheKey + "EXTENSIONS",
                   async () => (await DataReader.BulkReadRecordAsync<EdmExtensions>("EDM.EXTENSIONS", "")).ToList(), CacheTimeout);

            //await DataReader.BulkReadRecordAsync<EdmDepNotices>("EDM.DEP.NOTICES");
            // var extensionsData = await DataReader.BulkReadRecordAsync<EdmExtensions>("EDM.EXTENSIONS");

            foreach (var ethosExtensiblitySetting in ethosExtensiblitySettingsList)
            {
                var extendedEthosConfiguration = (await GetExtendedEthosConfigurationByResource(ethosExtensiblitySetting.EdmvResourceName, ethosExtensiblitySetting.EdmvVersionNumber,
                        ethosExtensiblitySetting.EdmvResourceName.ToUpperInvariant(), bypassCache));
                if (extendedEthosConfiguration != null)
                {
                    if (deprecatedData != null)
                    {
                        try
                        {
                            var depKey = string.Concat(extendedEthosConfiguration.ApiResourceName.ToUpper(), "*", extendedEthosConfiguration.ApiVersionNumber);
                            if (!string.IsNullOrEmpty(depKey))
                            {
                                //var depData = await DataReader.BulkReadRecordAsync<EdmDepNotices>("EDM.DEP.NOTICES", new string[] { depKey }, false);
                                var depData = deprecatedData.FirstOrDefault(x => x.Recordkey.Equals(depKey));
                                if (depData != null)
                                {
                                    extendedEthosConfiguration.DeprecationDate = depData.EdmpDeprecationDate;
                                    extendedEthosConfiguration.DeprecationNotice = depData.EdmpDeprecationNotice.Replace(_VM, ' ');
                                    extendedEthosConfiguration.SunsetDate = depData.EdmpSunsetDate;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            //do not throw error on supplemental data.
                            logger.Error(ex.Message, "Error on supplemental data.");
                        }
                    }

                    if (extensionsData != null)
                    {
                        try
                        {
                            //var extensions = await DataReader.BulkReadRecordAsync<EdmExtensions>("EDM.EXTENSIONS",
                            //    string.Format("WITH EDME.RESOURCE.NAME EQ '{0}'", extendedEthosConfiguration.ApiResourceName), false);
                            var extensionData = extensionsData.FirstOrDefault(ext => ext.EdmeResourceName.Equals(extendedEthosConfiguration.ApiResourceName));
                            if (extensionData != null)
                            {
                                extendedEthosConfiguration.HttpMethodsSupported = extensionData.EdmeHttpMethods;
                            }
                        }
                        catch (Exception ex)
                        {
                            //do not throw error on supplemental data.
                            logger.Error(ex.Message, "Error on supplemental data.");
                        }
                    }

                    retVal.Add(extendedEthosConfiguration);
                }
            }

            return retVal;
        }

        /// <summary>
        /// Gets all of the Ethos Extensiblity settings stored on EDM.EXT.VERSIONS for a specific resource 
        /// </summary>
        /// <param name="bypassCache">bool to determine if cache should be bypassed</param>
        /// <returns>List of Domain.Base.Entities.EthosExtensibleData</returns>
        public async Task<IEnumerable<Domain.Base.Entities.EthosExtensibleData>> GetEthosExtensibilityConfigurationEntitiesByResource(string resourceName, bool customOnly = true, bool bypassCache = false)
        {
            var retVal = new List<Domain.Base.Entities.EthosExtensibleData>();
            string ethosExtensiblityCacheKey = string.Concat("AllEthosExtensibiltySettingsAndData", resourceName);

            if (bypassCache && ContainsKey(BuildFullCacheKey(ethosExtensiblityCacheKey)))
            {
                ClearCache(new List<string> { ethosExtensiblityCacheKey + "VERSIONS" ,
                                              ethosExtensiblityCacheKey + "DEPRECATED",
                                              ethosExtensiblityCacheKey + "EXTENSIONS"});
            }

            List<string> edmeVersions = new List<string>();
            ICollection<EdmExtensions> extensionsData = null;
            if (customOnly)
                extensionsData = await DataReader.BulkReadRecordAsync<EdmExtensions>("EDM.EXTENSIONS", string.Format("WITH (EDME.PRIMARY.GUID.SOURCE NE '' OR EDME.PRIMARY.KEY NE '' OR EDME.PROCESS.ID NE '') AND EDME.RESOURCE.NAME EQ '{0}'", resourceName));
            else
                extensionsData = await DataReader.BulkReadRecordAsync<EdmExtensions>("EDM.EXTENSIONS", "");

            foreach (var ext in extensionsData)
            {
                edmeVersions.AddRange(ext.EdmeVersions);
            }

            var ethosExtensiblitySettingsList =
                await GetOrAddToCacheAsync<List<EdmExtVersions>>(ethosExtensiblityCacheKey + "VERSIONS",
                    async () => (await DataReader.BulkReadRecordAsync<EdmExtVersions>("EDM.EXT.VERSIONS", edmeVersions.ToArray(),
                        false)).ToList(), CacheTimeout);


            /*  var ethosExtensiblitySettingsList =
                await GetOrAddToCacheAsync<List<EdmExtVersions>>(ethosExtensiblityCacheKey + "VERSIONS",
                    async () => (await DataReader.BulkReadRecordAsync<EdmExtVersions>("EDM.EXT.VERSIONS", edmeVersions.Any() ? edmeVersions.ToArray() : null,
                        false)).ToList(), CacheTimeout); 
             */
            var deprecatedData =
                 await GetOrAddToCacheAsync<List<EdmDepNotices>>(ethosExtensiblityCacheKey + "DEPRECATED",
                    async () => (await DataReader.BulkReadRecordAsync<EdmDepNotices>("EDM.DEP.NOTICES", "")).ToList(), CacheTimeout);

            //var extensionsData =
            // await GetOrAddToCacheAsync<List<EdmExtensions>>(ethosExtensiblityCacheKey + "EXTENSIONS",
            // async () => (await DataReader.BulkReadRecordAsync<EdmExtensions>("EDM.EXTENSIONS", "")).ToList(), CacheTimeout);

                    //await DataReader.BulkReadRecordAsync<EdmDepNotices>("EDM.DEP.NOTICES");
                    // var extensionsData = await DataReader.BulkReadRecordAsync<EdmExtensions>("EDM.EXTENSIONS");

            foreach (var ethosExtensiblitySetting in ethosExtensiblitySettingsList)
            {
                var extendedEthosConfiguration = (await GetExtendedEthosConfigurationByResource(ethosExtensiblitySetting.EdmvResourceName, ethosExtensiblitySetting.EdmvVersionNumber,
                        ethosExtensiblitySetting.EdmvResourceName.ToUpperInvariant(),bypassCache,true));
                if (extendedEthosConfiguration != null)
                {
                    if (deprecatedData != null)
                    {
                        try
                        {
                            var depKey = string.Concat(extendedEthosConfiguration.ApiResourceName.ToUpper() + "*" + extendedEthosConfiguration.ApiVersionNumber);
                            if (!string.IsNullOrEmpty(depKey))
                            {
                                //var depData = await DataReader.BulkReadRecordAsync<EdmDepNotices>("EDM.DEP.NOTICES", new string[] { depKey }, false);
                                var depData = deprecatedData.FirstOrDefault(x => x.Recordkey.Equals(depKey));
                                if (depData != null)
                                {
                                    extendedEthosConfiguration.DeprecationDate = depData.EdmpDeprecationDate;
                                    extendedEthosConfiguration.DeprecationNotice = depData.EdmpDeprecationNotice.Replace(_VM, ' ');
                                    extendedEthosConfiguration.SunsetDate = depData.EdmpSunsetDate;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            //do not throw error on supplemental data.
                            logger.Error(ex.Message, "Error on supplemental data.");
                        }
                    }

                    if (extensionsData != null)
                    {
                        try
                        {
                            //var extensions = await DataReader.BulkReadRecordAsync<EdmExtensions>("EDM.EXTENSIONS",
                            //    string.Format("WITH EDME.RESOURCE.NAME EQ '{0}'", extendedEthosConfiguration.ApiResourceName), false);
                            var extensionData = extensionsData.FirstOrDefault(ext => ext.EdmeResourceName.Equals(extendedEthosConfiguration.ApiResourceName));
                            if (extensionData != null)
                            {
                                extendedEthosConfiguration.HttpMethodsSupported = extensionData.EdmeHttpMethods;
                            }
                        }
                        catch (Exception ex)
                        {
                            //do not throw error on supplemental data.
                            logger.Error(ex.Message, "Error on supplemental data.");
                        }
                    }

                    retVal.Add(extendedEthosConfiguration);
                }
            }

            return retVal;
        }

        /// <summary>
        /// Return the default version for an extension used in Stand Alone API builder.
        /// </summary>
        /// <param name="resourceName"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<string> GetEthosExtensibilityResourceDefaultVersion(string resourceName, bool bypassCache = false, string requestedVersion = "")
        {
            string ethosExtensiblityCacheKey = "AllExtendedEthosDefaultVersionBy" + resourceName + requestedVersion;

            if (bypassCache && ContainsKey(BuildFullCacheKey(ethosExtensiblityCacheKey)))
            {
                ClearCache(new List<string> { ethosExtensiblityCacheKey });
            }

            var defaultVersionNumber =
                await GetOrAddToCacheAsync(ethosExtensiblityCacheKey,
                    async () =>
                    {
                        string defaultVersion = string.Empty;
                        try
                        {
                            var extendConfigData = (await GetEthosExtensibilityConfiguration(resourceName, bypassCache)).ToList();
                            var matchingExtendedConfigData = extendConfigData.Where(e =>
                                e.EdmvResourceName.Equals(resourceName, StringComparison.OrdinalIgnoreCase));

                            List<EdmExtVersions> availableVersions = new List<EdmExtVersions>();
                            if (!string.IsNullOrEmpty(requestedVersion))
                            {
                                if (requestedVersion.Split('.').Count() > 2)
                                {
                                    return requestedVersion;
                                }
                                else
                                {
                                    availableVersions = matchingExtendedConfigData.Where(e => !string.IsNullOrEmpty(e.EdmvVersionNumber) && e.EdmvVersionNumber.StartsWith(requestedVersion)).OrderByDescending(n => n.EdmvVersionNumber.Split('.')[0]).ToList();
                                }
                            }
                            else
                            {
                                availableVersions = matchingExtendedConfigData.Where(e => !string.IsNullOrEmpty(e.EdmvVersionNumber)).OrderByDescending(n => n.EdmvVersionNumber.Split('.')[0]).ToList();
                            }
                            foreach (var availVersion in availableVersions)
                            {
                                if (string.IsNullOrEmpty(defaultVersion) && availVersion.EdmvVersionStatus != "B")
                                {
                                    if (string.IsNullOrEmpty(defaultVersion))
                                        defaultVersion = availVersion.EdmvVersionNumber;
                                }
                            }
                            if (string.IsNullOrEmpty(defaultVersion))
                            {
                                defaultVersion = availableVersions.FirstOrDefault().EdmvVersionNumber;
                                if (string.IsNullOrEmpty(defaultVersion) && string.IsNullOrEmpty(requestedVersion))
                                    defaultVersion = "1.0.0";
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Error(e, string.Concat("Retreiving Ethos Extended Confguration has failed for resource : ", resourceName));
                        }

                        return defaultVersion;

                    }, CacheTimeout);

            return defaultVersionNumber;
        }

        /// <summary>
        /// Gets the extended configuration available on a resource, returns null if there are none
        /// </summary>
        /// <param name="resourceName">name of the resource (api) </param>
        /// <param name="resourceVersionNumber">version number of ther resource</param>
        /// <param name="extendedSchemaResourceId">extended schema identifier</param>
        /// <returns> extended configuration if available. Returns null if none available or none configured</returns>
        public async Task<EthosExtensibleData> GetExtendedEthosConfigurationByResource(string resourceName, string resourceVersionNumber, string extendedSchemaResourceId, bool bypassCache = false, bool readRtFields = false)
        {
            string ethosExtensiblityCacheKey = "AllExtendedEthosConfigurationBy" + resourceName + resourceVersionNumber;

            if (bypassCache && ContainsKey(BuildFullCacheKey(ethosExtensiblityCacheKey)))
            {
                ClearCache(new List<string> { ethosExtensiblityCacheKey });
            }

            var ethosExtensibleData =
                await GetOrAddToCacheAsync(ethosExtensiblityCacheKey,
                    async () =>
                    {
                        try
                        {
                            var extendResourceConfigData = (await GetEthosApiConfiguration(resourceName, bypassCache)).ToList();

                            //make sure there is extended config data, if not return null
                            if (extendResourceConfigData == null || !extendResourceConfigData.Any())
                            {
                                return new EthosExtensibleData();

                            }

                            var apiConfiguration = extendResourceConfigData.FirstOrDefault(ex => ex.EdmeResourceName == resourceName);

                            //make sure there is extended config data for this resource, if not return null
                            if (apiConfiguration == null)
                            {
                                return new EthosExtensibleData();

                            }

                            var parentApi = apiConfiguration.EdmeParentApi;

                            var extendVersionConfigData = (await GetEthosExtensibilityConfiguration(resourceName, bypassCache)).ToList();

                            //make sure there is extended version config data, if not return null
                            if (extendVersionConfigData == null || !extendVersionConfigData.Any())
                            {
                                return new EthosExtensibleData();

                            }

                            // Look for version specific configuration first
                            var matchingExtendedConfigData = extendVersionConfigData.FirstOrDefault(e =>
                                e.EdmvResourceName.Equals(resourceName, StringComparison.OrdinalIgnoreCase) &&
                                e.EdmvVersionNumber.Equals(resourceVersionNumber, StringComparison.OrdinalIgnoreCase));

                            // Look for a major version match.
                            if (matchingExtendedConfigData == null)
                            {
                                if (!string.IsNullOrEmpty(resourceVersionNumber) && resourceVersionNumber.Contains('.'))
                                {
                                    var majorVersion = resourceVersionNumber.Split('.')[0];
                                    var minorVersion = resourceVersionNumber.Split('.')[1];
                                    matchingExtendedConfigData = extendVersionConfigData.LastOrDefault(e =>
                                        e.EdmvResourceName.Equals(resourceName, StringComparison.OrdinalIgnoreCase) &&
                                        e.EdmvVersionNumber.Equals(majorVersion, StringComparison.OrdinalIgnoreCase));

                                    if (matchingExtendedConfigData == null)
                                    {
                                        matchingExtendedConfigData = extendVersionConfigData.LastOrDefault(e =>
                                            e.EdmvResourceName.Equals(resourceName, StringComparison.OrdinalIgnoreCase) &&
                                            e.EdmvVersionNumber.Contains('.') &&
                                            e.EdmvVersionNumber.Split('.')[0].Equals(majorVersion, StringComparison.OrdinalIgnoreCase) &&
                                            Convert.ToInt32(e.EdmvVersionNumber.Split('.')[1]) >= Convert.ToInt32(minorVersion));
                                    }
                                }
                            }

                            // If we don't have a version specific or major version configuration, then look for versionless match.
                            if (matchingExtendedConfigData == null)
                            {
                                matchingExtendedConfigData = extendVersionConfigData.FirstOrDefault(e =>
                                    e.EdmvResourceName.Equals(resourceName, StringComparison.OrdinalIgnoreCase) &&
                                    string.IsNullOrEmpty(e.EdmvVersionNumber));
                            }

                            //make sure there is extended config data and row data, if not return null
                            if (matchingExtendedConfigData == null)
                            {
                                return new EthosExtensibleData();

                            }

                            string versionNumber = resourceVersionNumber;
                            if (resourceVersionNumber != matchingExtendedConfigData.EdmvVersionNumber)
                            {
                                versionNumber = versionNumber.Split('.')[0];
                                if (!string.IsNullOrEmpty(matchingExtendedConfigData.EdmvVersionNumber))
                                {
                                    versionNumber = matchingExtendedConfigData.EdmvVersionNumber;
                                }
                            }


                            if (matchingExtendedConfigData.EdmvColumnsEntityAssociation == null ||
                                !matchingExtendedConfigData.EdmvColumnsEntityAssociation.Any())
                            {
                                return new EthosExtensibleData(resourceName, versionNumber,
                                 matchingExtendedConfigData.EdmvExtendedSchemaType, string.Empty, colleagueTimeZone);
                            }


                            //create EthosExtensibleData with no resource id since this is just to get the config data for each row
                            var extendedDataConfigReturn = new EthosExtensibleData(resourceName, versionNumber,
                                matchingExtendedConfigData.EdmvExtendedSchemaType, string.Empty, colleagueTimeZone);
                            extendedDataConfigReturn.ApiType = apiConfiguration.EdmeType;
                            extendedDataConfigReturn.ParentApi = parentApi;
                            extendedDataConfigReturn.Description = matchingExtendedConfigData.EdmvDescription;
                            extendedDataConfigReturn.CurrentUserIdPath = apiConfiguration.EdmeCurrentUserPath;
                            extendedDataConfigReturn.ColleagueFileNames = apiConfiguration.EdmeResourceEntities;
                            extendedDataConfigReturn.ColleagueKeyNames = apiConfiguration.EdmeNkElementName;
                            extendedDataConfigReturn.InquiryFields = matchingExtendedConfigData.EdmvInquiryFields;
                            extendedDataConfigReturn.VersionReleaseStatus = string.IsNullOrEmpty(matchingExtendedConfigData.EdmvVersionStatus) ? apiConfiguration.EdmeApiStatus : matchingExtendedConfigData.EdmvVersionStatus;
                            extendedDataConfigReturn.IsCustomResource = true;
                            if (apiConfiguration.EdmeType.Equals("T", StringComparison.OrdinalIgnoreCase)) extendedDataConfigReturn.IsCustomResource = false;
                            if (apiConfiguration.EdmeType.Equals("S", StringComparison.OrdinalIgnoreCase)) extendedDataConfigReturn.IsCustomResource = false;
                            if (matchingExtendedConfigData.Recordkey.StartsWith("d01", StringComparison.OrdinalIgnoreCase)) extendedDataConfigReturn.IsCustomResource = false;

                            //list of linked column config data for datetime columns, exlcuding date or time only ones.
                            var linkedColumnDetails = matchingExtendedConfigData.EdmvColumnsEntityAssociation
                                .Where(l => !string.IsNullOrEmpty(l.EdmvDateTimeLinkAssocMember) &&
                                            !l.EdmvJsonPropertyTypeAssocMember.Equals("date", StringComparison.OrdinalIgnoreCase)).ToList();


                            //loop through and add each row
                            foreach (var edmvColumn in matchingExtendedConfigData.EdmvColumnsEntityAssociation)
                            {
                                var absoluteColumnName = edmvColumn.EdmvColumnNameAssocMember;
                                if (absoluteColumnName.Contains(':'))
                                {
                                    var splitColumnNames = absoluteColumnName.Split(':');
                                    int idx = splitColumnNames.Count();
                                    if (idx > 0) absoluteColumnName = splitColumnNames[idx - 1];
                                }
                                //if the datetime link assoc has a value it is part of a pair
                                //only add the value that is marked as the datetime
                                if (!string.IsNullOrEmpty(edmvColumn.EdmvDateTimeLinkAssocMember))
                                {
                                    if (!edmvColumn.EdmvConversionAssocMember.StartsWith("D", StringComparison.OrdinalIgnoreCase)) continue;

                                    var row = new EthosExtensibleDataRow(edmvColumn.EdmvColumnNameAssocMember, edmvColumn.EdmvFileNameAssocMember,
                                        edmvColumn.EdmvJsonLabelAssocMember, edmvColumn.EdmvJsonPathAssocMember,
                                        edmvColumn.EdmvJsonPropertyTypeAssocMember, "", edmvColumn.EdmvLengthAssocMember)
                                    {
                                        AssociationController = edmvColumn.EdmvAssociationControllerAssocMember,
                                        TransType = edmvColumn.EdmvTransTypeAssocMember,
                                        DatabaseUsageType = edmvColumn.EdmvDatabaseUsageTypeAssocMember,
                                        ColleaguePropertyPosition = edmvColumn.EdmvFieldNumberAssocMember,
                                        Required = edmvColumn.EdmvColumnRequiredAssocMember.Equals("Y") ? true : false,
                                        Description = edmvColumn.EdmvColumnDescAssocMember,
                                        Conversion = edmvColumn.EdmvConversionAssocMember
                                    };

                                    extendedDataConfigReturn.AddItemToExtendedData(row);

                                    var timeConfig = linkedColumnDetails.FirstOrDefault(t =>
                                        t.EdmvDateTimeLinkAssocMember == edmvColumn.EdmvDateTimeLinkAssocMember &&
                                        t.EdmvConversionAssocMember.StartsWith("M"));

                                    if (timeConfig != null)
                                    {
                                        var timeRow = new EthosExtensibleDataRow(timeConfig.EdmvColumnNameAssocMember, timeConfig.EdmvFileNameAssocMember,
                                            edmvColumn.EdmvJsonLabelAssocMember, edmvColumn.EdmvJsonPathAssocMember,
                                            edmvColumn.EdmvJsonPropertyTypeAssocMember, "", timeConfig.EdmvLengthAssocMember)
                                        {
                                            AssociationController = timeConfig.EdmvAssociationControllerAssocMember,
                                            TransType = timeConfig.EdmvTransTypeAssocMember,
                                            DatabaseUsageType = timeConfig.EdmvDatabaseUsageTypeAssocMember,
                                            ColleaguePropertyPosition = edmvColumn.EdmvFieldNumberAssocMember,
                                            Required = edmvColumn.EdmvColumnRequiredAssocMember.Equals("Y") ? true : false,
                                            Description = timeConfig.EdmvColumnDescAssocMember,
                                            Conversion = edmvColumn.EdmvConversionAssocMember
                                        };

                                        extendedDataConfigReturn.AddItemToExtendedData(timeRow);
                                    }
                                }
                                else
                                {
                                    var row = new EthosExtensibleDataRow(edmvColumn.EdmvColumnNameAssocMember, edmvColumn.EdmvFileNameAssocMember,
                                        edmvColumn.EdmvJsonLabelAssocMember, edmvColumn.EdmvJsonPathAssocMember,
                                        edmvColumn.EdmvJsonPropertyTypeAssocMember, "", edmvColumn.EdmvLengthAssocMember)
                                    {
                                        AssociationController = edmvColumn.EdmvAssociationControllerAssocMember,
                                        TransType = edmvColumn.EdmvTransTypeAssocMember,
                                        DatabaseUsageType = edmvColumn.EdmvDatabaseUsageTypeAssocMember,
                                        ColleaguePropertyPosition = edmvColumn.EdmvFieldNumberAssocMember,
                                        Required = edmvColumn.EdmvColumnRequiredAssocMember.Equals("Y") ? true : false,
                                        Description = edmvColumn.EdmvColumnDescAssocMember,
                                        Conversion = edmvColumn.EdmvConversionAssocMember
                                    };
                                    //add validation info
                                    row.TransFileName = edmvColumn.EdmvTransFileNameAssocMember;
                                    row.TransTableName = edmvColumn.EdmvTransTableNameAssocMember;
                                    row.TransColumnName = edmvColumn.EdmvTransColumnNameAssocMember;

                                    //if there is still no validation, get it from RT.FIELDS
                                    if (readRtFields && apiConfiguration.EdmeType != "T" && string.IsNullOrEmpty(row.TransFileName) )
                                    {
                                        if (string.IsNullOrEmpty(row.TransFileName))
                                        {
                                            var fieldName = string.Empty;
                                            if (!string.IsNullOrEmpty(row.ColleagueColumnName))
                                            {
                                                if (row.ColleagueColumnName.Contains(":"))
                                                {
                                                    var colnames = row.ColleagueColumnName.Split(':');
                                                    var count = row.ColleagueColumnName.Count(x => x == ':');
                                                    fieldName = colnames[count];
                                                }
                                                else
                                                {
                                                    fieldName = row.ColleagueColumnName;
                                                }
                                            }

                                            if (!string.IsNullOrEmpty(fieldName))
                                            {
                                                var record = await DataReader.ReadRecordAsync<RtFields>("RT.FIELDS", fieldName);
                                                if (record != null)
                                                {
                                                    if (!string.IsNullOrEmpty(record.RtfldsValTableApplication) && !string.IsNullOrEmpty(record.RtfldsValidationTable))
                                                    {
                                                        row.TransFileName = string.Concat(record.RtfldsValTableApplication, ".VALCODES - ", record.RtfldsValidationTable);
                                                    }
                                                    else if (!string.IsNullOrEmpty(record.RtfldsValidationFile))
                                                    {
                                                        row.TransFileName = record.RtfldsValidationFile;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    extendedDataConfigReturn.AddItemToExtendedData(row);
                                }
                                // If we don't have a select file name, then skip this property for filtering
                                if (string.IsNullOrEmpty(edmvColumn.EdmvFileNameAssocMember))
                                {
                                    continue;
                                }
                                // Add Filter Criteria to configuration
                                EdmSelectCriteria selectCriteria = null;
                                if (!string.IsNullOrEmpty(edmvColumn.EdmvFilterCriteriaAssocMember))
                                {
                                    selectCriteria = (await GetEthosApiSelectCriteria(edmvColumn.EdmvFilterCriteriaAssocMember, bypassCache));
                                }
                                else
                                {
                                    // If we are pointing to a different table, then we can't select
                                    // from the primary table and the selection criteria is too complicated
                                    // to simply select with column equal to value.
                                    if (string.IsNullOrEmpty(edmvColumn.EdmvSecColumnNameAssocMember))
                                    {
                                        if (!edmvColumn.EdmvFileNameAssocMember.EndsWith("VALCODES"))
                                        {
                                            selectCriteria = new EdmSelectCriteria()
                                            {
                                                EdmsSelectFileName = edmvColumn.EdmvFileNameAssocMember,
                                                EdmsSelectColumnName = edmvColumn.EdmvJsonLabelAssocMember.TrimEnd(']').TrimEnd('['),
                                                EdmsSelectEntityAssociation = new List<EdmSelectCriteriaEdmsSelect>()
                                                {
                                                    new EdmSelectCriteriaEdmsSelect()
                                                    {
                                                        EdmsSelectColumnAssocMember = absoluteColumnName,
                                                        EdmsSelectConnectorAssocMember = "WITH",
                                                        EdmsSelectOperAssocMember = "EQ",
                                                        EdmsSelectValueAssocMember = '"' + edmvColumn.EdmvJsonLabelAssocMember.TrimEnd(']').TrimEnd('[') + '"'
                                                    }
                                                },
                                                EdmsAdvanceQueryOpers = new List<string>(),
                                                EdmsSortEntityAssociation = new List<EdmSelectCriteriaEdmsSort>()
                                            };
                                            if (!string.IsNullOrEmpty(apiConfiguration.EdmePrimaryGuidSource) && !apiConfiguration.EdmePrimaryGuidDbType.Equals("K", StringComparison.OrdinalIgnoreCase))
                                            {
                                                selectCriteria.EdmsSelectFileName = apiConfiguration.EdmePrimaryEntity;
                                                selectCriteria.EdmsSavingField = apiConfiguration.EdmePrimaryGuidSource;
                                                selectCriteria.EdmsSelectEntityAssociation.Add(
                                                    new EdmSelectCriteriaEdmsSelect()
                                                    {
                                                        EdmsSelectColumnAssocMember = apiConfiguration.EdmePrimaryGuidSource,
                                                        EdmsSelectConnectorAssocMember = "WITH",
                                                        EdmsSelectOperAssocMember = "NE",
                                                        EdmsSelectValueAssocMember = "''"
                                                    }
                                                );
                                                selectCriteria.EdmsSortEntityAssociation = new List<EdmSelectCriteriaEdmsSort>()
                                                {
                                                    new EdmSelectCriteriaEdmsSort(apiConfiguration.EdmePrimaryGuidSource, "BY.EXP")
                                                };
                                            }
                                        }
                                        else
                                        {
                                            if (!string.IsNullOrEmpty(apiConfiguration.EdmePrimaryTableName))
                                            {
                                                string fileName = edmvColumn.EdmvFileNameAssocMember;
                                                if (!string.IsNullOrEmpty(apiConfiguration.EdmePrimaryApplication))
                                                {
                                                    fileName = await ValidateValcodeTable(apiConfiguration.EdmePrimaryApplication, apiConfiguration.EdmePrimaryTableName);
                                                }
                                                selectCriteria = new EdmSelectCriteria()
                                                {
                                                    EdmsSelectFileName = fileName,
                                                    EdmsSelectColumnName = edmvColumn.EdmvJsonLabelAssocMember.TrimEnd(']').TrimEnd('['),
                                                    EdmsSelectEntityAssociation = new List<EdmSelectCriteriaEdmsSelect>()
                                                    {
                                                        new EdmSelectCriteriaEdmsSelect()
                                                        {
                                                            EdmsSelectColumnAssocMember = "VALCODE.ID",
                                                            EdmsSelectConnectorAssocMember = "WITH",
                                                            EdmsSelectOperAssocMember = "EQ",
                                                            EdmsSelectValueAssocMember = '"' + apiConfiguration.EdmePrimaryTableName + '"'
                                                        },
                                                        new EdmSelectCriteriaEdmsSelect()
                                                        {
                                                            EdmsSelectColumnAssocMember = absoluteColumnName,
                                                            EdmsSelectConnectorAssocMember = "WITH",
                                                            EdmsSelectOperAssocMember = "EQ",
                                                            EdmsSelectValueAssocMember = '"' + edmvColumn.EdmvJsonLabelAssocMember + '"'
                                                        }
                                                    },
                                                    EdmsAdvanceQueryOpers = new List<string>(),
                                                    EdmsSavingField = "VAL.INTERNAL.CODE",
                                                    EdmsSortEntityAssociation = new List<EdmSelectCriteriaEdmsSort>()
                                                    {
                                                        new EdmSelectCriteriaEdmsSort("VAL.INTERNAL.CODE", "BY.EXP")
                                                    }
                                                };
                                            }
                                        }
                                    }
                                }
                                if (selectCriteria != null)
                                {
                                    var filterRow = new EthosExtensibleDataFilter(absoluteColumnName, edmvColumn.EdmvFileNameAssocMember,
                                            edmvColumn.EdmvJsonLabelAssocMember, edmvColumn.EdmvJsonPathAssocMember,
                                            edmvColumn.EdmvJsonPropertyTypeAssocMember, new List<string>());

                                    filterRow.DatabaseUsageType = edmvColumn.EdmvDatabaseUsageTypeAssocMember;
                                    filterRow.ColleagueFieldPosition = edmvColumn.EdmvFieldNumberAssocMember;
                                    filterRow.Required = edmvColumn.EdmvColumnRequiredAssocMember == "Y" ? true : false;
                                    filterRow.SelectFileName = selectCriteria.EdmsSelectFileName;
                                    filterRow.SelectSubroutineName = selectCriteria.EdmsSelectSubroutine;
                                    filterRow.SavingField = selectCriteria.EdmsSavingField;
                                    filterRow.SavingOption = selectCriteria.EdmsSavingOption;
                                    filterRow.SelectColumnName = selectCriteria.EdmsSelectColumnName;
                                    filterRow.SelectRules = selectCriteria.EdmsSelectRules;
                                    filterRow.SelectParagraph = selectCriteria.EdmsSelectParagraph;
                                    filterRow.ValidFilterOpers = selectCriteria.EdmsAdvanceQueryOpers;
                                    if (apiConfiguration.EdmeNkElementName.Contains(absoluteColumnName))
                                    {
                                        filterRow.KeyQuery = true;
                                    }

                                    filterRow.SelectionCriteria = new List<EthosApiSelectCriteria>();
                                    foreach (var selection in selectCriteria.EdmsSelectEntityAssociation)
                                    {
                                        if (!string.IsNullOrEmpty(selection.EdmsSelectConnectorAssocMember))
                                        {
                                            filterRow.SelectionCriteria.Add(
                                                new EthosApiSelectCriteria(selection.EdmsSelectConnectorAssocMember,
                                                selection.EdmsSelectColumnAssocMember,
                                                selection.EdmsSelectOperAssocMember,
                                                selection.EdmsSelectValueAssocMember)
                                            );
                                        }
                                    }

                                    filterRow.SortColumns = new List<EthosApiSortCriteria>();
                                    foreach (var sort in selectCriteria.EdmsSortEntityAssociation)
                                    {
                                        if (!string.IsNullOrEmpty(sort.EdmsSortColumnsAssocMember))
                                        {
                                            filterRow.SortColumns.Add(
                                                new EthosApiSortCriteria(sort.EdmsSortColumnsAssocMember,
                                                sort.EdmsSortSequenceAssocMember)
                                            );
                                        }
                                    }

                                    filterRow.GuidColumnName = edmvColumn.EdmvGuidColumnNameAssocMember;
                                    filterRow.GuidFileName = edmvColumn.EdmvGuidFileNameAssocMember;
                                    filterRow.TransColumnName = edmvColumn.EdmvTransColumnNameAssocMember;
                                    filterRow.TransFileName = edmvColumn.EdmvTransFileNameAssocMember;
                                    filterRow.TransTableName = edmvColumn.EdmvTransTableNameAssocMember;
                                    filterRow.Enumerations = new List<EthosApiEnumerations>();
                                    var enums = edmvColumn.EdmvTransEnumTableAssocMember;
                                    var enumTable = enums.Split(_SM);
                                    if (enumTable != null && enumTable.Count() == 2)
                                    {
                                        var collValue = enumTable[0].Split(_TM);
                                        var enumValue = enumTable[1].Split(_TM);
                                        int total = collValue.Count() > enumValue.Count() ? collValue.Count() : enumValue.Count();
                                        for (int i = 0; i < total; i++)
                                        {
                                            string value1 = enumValue.Count() > i ? enumValue[i] : string.Empty;
                                            string value2 = collValue.Count() > i ? collValue[i] : string.Empty;
                                            if (!string.IsNullOrEmpty(value1) || !string.IsNullOrEmpty(value2))
                                            {
                                                filterRow.Enumerations.Add(new EthosApiEnumerations(value1, value2));
                                            }
                                        }
                                    }
                                    extendedDataConfigReturn.AddItemToExtendedDataFilter(filterRow);
                                }
                            }

                            // Add columns for prepared responses
                            foreach (var prmpt in apiConfiguration.EdmePreparedResponsesEntityAssociation)
                            {
                                if (!string.IsNullOrEmpty(prmpt.EdmePromptResponseTextAssocMember) && !string.IsNullOrEmpty(prmpt.EdmePromptResponseTitleAssocMember))
                                {
                                    var columnName = prmpt.EdmePromptResponseTitleAssocMember.ToUpper();
                                    var path = string.Concat("/predefinedInputs/");
                                    var row = new EthosExtensibleDataRow(columnName, "", prmpt.EdmePromptResponseTitleAssocMember, path, "string", "")
                                    {
                                        Required = false,
                                        Description = prmpt.EdmePromptResponseTextAssocMember,
                                        TransFileName = prmpt.EdmePromptResponseOptsAssocMember,
                                        TransType = prmpt.EdmePromptResponseDfltAssocMember
                                    };
                                    extendedDataConfigReturn.AddItemToExtendedData(row);
                                }
                            }

                            // Add Named Queries to configuration
                            if (matchingExtendedConfigData.EdmvNamedQueries != null && matchingExtendedConfigData.EdmvNamedQueries.Any())
                            {
                                foreach (var edmQuery in matchingExtendedConfigData.EdmvNamedQueries)
                                {
                                    var queryData = await GetEthosApiNamedQueries(edmQuery, bypassCache);
                                    var selectCriteria = await GetEthosApiSelectCriteria(queryData.EdmqSelectCriteria, bypassCache);
                                    if (queryData != null && selectCriteria != null)
                                    {
                                        var filterRow = new EthosExtensibleDataFilter(queryData.EdmqJsonLabel, "",
                                                queryData.EdmqJsonLabel, "",
                                                queryData.EdmqJsonPropertyType, new List<string>(), null, true);

                                        filterRow.SelectFileName = selectCriteria.EdmsSelectFileName;
                                        filterRow.SelectSubroutineName = selectCriteria.EdmsSelectSubroutine;
                                        filterRow.SavingField = selectCriteria.EdmsSavingField;
                                        filterRow.SavingOption = selectCriteria.EdmsSavingOption;
                                        filterRow.SelectColumnName = selectCriteria.EdmsSelectColumnName;
                                        filterRow.SelectRules = selectCriteria.EdmsSelectRules;
                                        filterRow.SelectParagraph = selectCriteria.EdmsSelectParagraph;
                                        filterRow.ValidFilterOpers = selectCriteria.EdmsAdvanceQueryOpers;                                        
                                        filterRow.SelectionCriteria = new List<EthosApiSelectCriteria>();
                                        foreach (var selection in selectCriteria.EdmsSelectEntityAssociation)
                                        {
                                            if (!string.IsNullOrEmpty(selection.EdmsSelectConnectorAssocMember))
                                            {
                                                filterRow.SelectionCriteria.Add(
                                                    new EthosApiSelectCriteria(selection.EdmsSelectConnectorAssocMember,
                                                    selection.EdmsSelectColumnAssocMember,
                                                    selection.EdmsSelectOperAssocMember,
                                                    selection.EdmsSelectValueAssocMember)
                                                );
                                            }
                                        }

                                        filterRow.SortColumns = new List<EthosApiSortCriteria>();
                                        foreach (var sort in selectCriteria.EdmsSortEntityAssociation)
                                        {
                                            if (!string.IsNullOrEmpty(sort.EdmsSortColumnsAssocMember))
                                            {
                                                filterRow.SortColumns.Add(
                                                    new EthosApiSortCriteria(sort.EdmsSortColumnsAssocMember,
                                                    sort.EdmsSortSequenceAssocMember)
                                                );
                                            }
                                        }

                                        filterRow.GuidColumnName = queryData.EdmqGuidColumnName;
                                        filterRow.GuidFileName = queryData.EdmqGuidFileName;
                                        filterRow.TransColumnName = queryData.EdmqTransColumnName;
                                        filterRow.TransFileName = queryData.EdmqTransFileName;
                                        filterRow.TransTableName = queryData.EdmqTransTableName;
                                        filterRow.Description = queryData.EdmqDescription;
                                        filterRow.Enumerations = new List<EthosApiEnumerations>();
                                        var collValue = queryData.EdmqTransEnumValue;
                                        var enumValue = queryData.EdmqTransCollValue;
                                        int total = collValue.Count() > enumValue.Count() ? collValue.Count() : enumValue.Count();
                                        for (int i = 0; i < total; i++)
                                        {
                                            string value1 = enumValue.Count() < i ? enumValue.ElementAt(i) : string.Empty;
                                            string value2 = collValue.Count() < i ? collValue.ElementAt(i) : string.Empty;
                                            if (!string.IsNullOrEmpty(value1) || !string.IsNullOrEmpty(value2))
                                            {
                                                filterRow.Enumerations.Add(new EthosApiEnumerations(value1, value2));
                                            }
                                        }
                                        extendedDataConfigReturn.AddItemToExtendedDataFilter(filterRow);
                                    }
                                }
                            }

                            // Add key names to filter table
                            if (apiConfiguration.EdmeNkElementName != null && apiConfiguration.EdmeNkElementName.Any())
                            {
                                foreach (var columnName in apiConfiguration.EdmeNkElementName)
                                {
                                    var edmvColumn = matchingExtendedConfigData.EdmvColumnsEntityAssociation.FirstOrDefault(ea => ea.EdmvColumnNameAssocMember == columnName);
                                    var existingFilter = extendedDataConfigReturn.ExtendedDataFilterList.FirstOrDefault(edf => edf.ColleagueColumnName == columnName && edf.JsonPath == "/"); 
                                    if (edmvColumn != null && existingFilter == null)
                                    {
                                        var filterRow = new EthosExtensibleDataFilter(edmvColumn.EdmvColumnNameAssocMember, edmvColumn.EdmvFileNameAssocMember,
                                            edmvColumn.EdmvJsonLabelAssocMember, "/",
                                            edmvColumn.EdmvJsonPropertyTypeAssocMember, new List<string>(), null, false, true);

                                        filterRow.DatabaseUsageType = edmvColumn.EdmvDatabaseUsageTypeAssocMember;
                                        filterRow.ColleagueFieldPosition = edmvColumn.EdmvFieldNumberAssocMember;
                                        filterRow.Required = edmvColumn.EdmvColumnRequiredAssocMember == "Y" ? true : false;

                                        filterRow.SelectFileName = edmvColumn.EdmvFileNameAssocMember;
                                        filterRow.ColleagueFieldPosition = edmvColumn.EdmvFieldNumberAssocMember;
                                        filterRow.GuidColumnName = edmvColumn.EdmvGuidColumnNameAssocMember;
                                        filterRow.GuidFileName = edmvColumn.EdmvGuidFileNameAssocMember;
                                        filterRow.TransColumnName = edmvColumn.EdmvTransColumnNameAssocMember;
                                        filterRow.TransFileName = edmvColumn.EdmvTransFileNameAssocMember;
                                        filterRow.TransTableName = edmvColumn.EdmvTransTableNameAssocMember;
                                        filterRow.Enumerations = new List<EthosApiEnumerations>();
                                        var enums = edmvColumn.EdmvTransEnumTableAssocMember;
                                        var enumTable = enums.Split(_SM);
                                        if (enumTable != null && enumTable.Count() == 2)
                                        {
                                            var collValue = enumTable[0].Split(_TM);
                                            var enumValue = enumTable[1].Split(_TM);
                                            int total = collValue.Count() > enumValue.Count() ? collValue.Count() : enumValue.Count();
                                            for (int i = 0; i < total; i++)
                                            {
                                                string value1 = enumValue.Count() > i ? enumValue[i] : string.Empty;
                                                string value2 = collValue.Count() > i ? collValue[i] : string.Empty;
                                                if (!string.IsNullOrEmpty(value1) || !string.IsNullOrEmpty(value2))
                                                {
                                                    filterRow.Enumerations.Add(new EthosApiEnumerations(value1, value2));
                                                }
                                            }
                                        }
                                        extendedDataConfigReturn.AddItemToExtendedDataFilter(filterRow);
                                    }
                                    existingFilter = extendedDataConfigReturn.ExtendedDataFilterList.FirstOrDefault(edf => edf.ColleagueColumnName == columnName && edf.FullJsonPath.StartsWith("/id/"));
                                    if (edmvColumn != null && existingFilter == null)
                                    {
                                        var filterRow = new EthosExtensibleDataFilter(edmvColumn.EdmvColumnNameAssocMember, edmvColumn.EdmvFileNameAssocMember,
                                            edmvColumn.EdmvJsonLabelAssocMember, "/id/",
                                            edmvColumn.EdmvJsonPropertyTypeAssocMember, new List<string>(), null, false, true);

                                        filterRow.DatabaseUsageType = edmvColumn.EdmvDatabaseUsageTypeAssocMember;
                                        filterRow.ColleagueFieldPosition = edmvColumn.EdmvFieldNumberAssocMember;
                                        filterRow.Required = edmvColumn.EdmvColumnRequiredAssocMember == "Y" ? true : false;

                                        filterRow.SelectFileName = edmvColumn.EdmvFileNameAssocMember;
                                        filterRow.ColleagueFieldPosition = edmvColumn.EdmvFieldNumberAssocMember;
                                        filterRow.GuidColumnName = edmvColumn.EdmvGuidColumnNameAssocMember;
                                        filterRow.GuidFileName = edmvColumn.EdmvGuidFileNameAssocMember;
                                        filterRow.TransColumnName = edmvColumn.EdmvTransColumnNameAssocMember;
                                        filterRow.TransFileName = edmvColumn.EdmvTransFileNameAssocMember;
                                        filterRow.TransTableName = edmvColumn.EdmvTransTableNameAssocMember;
                                        filterRow.Enumerations = new List<EthosApiEnumerations>();
                                        var enums = edmvColumn.EdmvTransEnumTableAssocMember;
                                        var enumTable = enums.Split(_SM);
                                        if (enumTable != null && enumTable.Count() == 2)
                                        {
                                            var collValue = enumTable[0].Split(_TM);
                                            var enumValue = enumTable[1].Split(_TM);
                                            int total = collValue.Count() > enumValue.Count() ? collValue.Count() : enumValue.Count();
                                            for (int i = 0; i < total; i++)
                                            {
                                                string value1 = enumValue.Count() > i ? enumValue[i] : string.Empty;
                                                string value2 = collValue.Count() > i ? collValue[i] : string.Empty;
                                                if (!string.IsNullOrEmpty(value1) || !string.IsNullOrEmpty(value2))
                                                {
                                                    filterRow.Enumerations.Add(new EthosApiEnumerations(value1, value2));
                                                }
                                            }
                                        }                                       
                                        extendedDataConfigReturn.AddItemToExtendedDataFilter(filterRow);
                                    }
                                }
                            }

                            if (extendedDataConfigReturn.ExtendedDataList != null &&
                                extendedDataConfigReturn.ExtendedDataList.Any())
                            {
                                return extendedDataConfigReturn;
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Error(e, string.Concat("Retreiving Ethos Extended Confguration has failed for resource : ", resourceName, " at version : ", resourceVersionNumber));
                        }

                        //if it makes it here there were no configuration rows or an exception so return null
                        return new EthosExtensibleData();
                    }, CacheTimeout);

            if (ethosExtensibleData == null || ethosExtensibleData.ExtendedDataList == null || !ethosExtensibleData.ExtendedDataList.Any())
            {
                return null;
            }
            return ethosExtensibleData;

        }

        private IEnumerable<EdmExtensions> _ethosExtensibilityByResourceSettings = null;
        /// <summary>
        /// Gets all of the Ethos API builder settings stored on EDM.EXTENSIONS
        /// </summary>
        /// <param name="bypassCache">bool to determine if cache should be bypassed</param>
        /// <returns>List of DataContracts.EdmExtensions</returns>
        private async Task<IEnumerable<EdmExtensions>> GetEthosApiConfiguration(string resource, bool bypassCache = false)
        {
            if (_ethosExtensibilityByResourceSettings == null)
            {
                string ethosExtensiblityCacheKey = "EthosApiBuilderConfigurationSettingsByResource" + resource;

                if (bypassCache && ContainsKey(BuildFullCacheKey(ethosExtensiblityCacheKey)))
                {
                    ClearCache(new List<string> { ethosExtensiblityCacheKey });
                }
                string criteria = string.Format("WITH EDME.RESOURCE.NAME EQ '{0}'", resource);
                var ethosExtensiblitySettingsList =
                    await GetOrAddToCacheAsync(ethosExtensiblityCacheKey,
                        async () => (await DataReader.BulkReadRecordAsync<EdmExtensions>("EDM.EXTENSIONS", criteria)).ToList(), CacheTimeout);

                _ethosExtensibilityByResourceSettings = ethosExtensiblitySettingsList;
            }
            return _ethosExtensibilityByResourceSettings;
        }

        /// <summary>
        /// Gets all of the Ethos API builder settings stored on EDM.SELECT.CRITERIA
        /// </summary>
        /// <param name="bypassCache">bool to determine if cache should be bypassed</param>
        /// <returns>List of DataContracts.EdmExtensions</returns>
        private async Task<EdmSelectCriteria> GetEthosApiSelectCriteria(string id, bool bypassCache = false)
        {
            string ethosExtensiblityCacheKey = "EthosApiSelectCriteriaConfigurationSettingsById" + id;

            if (bypassCache && ContainsKey(BuildFullCacheKey(ethosExtensiblityCacheKey)))
            {
                ClearCache(new List<string> { ethosExtensiblityCacheKey });
            }

            return await GetOrAddToCacheAsync<EdmSelectCriteria>(ethosExtensiblityCacheKey,
                    async () => (await DataReader.ReadRecordAsync<EdmSelectCriteria>("EDM.SELECT.CRITERIA", id)), CacheTimeout);
        }

        /// <summary>
        /// Gets all of the Ethos API builder settings stored on EDM.QUERIES
        /// </summary>
        /// <param name="bypassCache">bool to determine if cache should be bypassed</param>
        /// <returns>List of DataContracts.EdmExtensions</returns>
        private async Task<EdmQueries> GetEthosApiNamedQueries(string id, bool bypassCache = false)
        {
            string ethosExtensiblityCacheKey = "EthosApiNamedQueriesConfigurationSettingsById" + id;

            if (bypassCache && ContainsKey(BuildFullCacheKey(ethosExtensiblityCacheKey)))
            {
                ClearCache(new List<string> { ethosExtensiblityCacheKey });
            }

            return await GetOrAddToCacheAsync<EdmQueries>(ethosExtensiblityCacheKey,
                    async () => (await DataReader.ReadRecordAsync<EdmQueries>("EDM.QUERIES", id)), CacheTimeout);
        }

        /// <summary>
        /// Gets the extended configuration available on a resource, returns null if there are none
        /// </summary>
        /// <param name="resourceName">name of the resource (api) </param>
        /// <returns> extended configuration if available. Returns null if none available or none configured</returns>
        public async Task<EthosApiConfiguration> GetEthosApiConfigurationByResource(string resourceName, bool bypassCache = false)
        {
            string ethosExtensiblityCacheKey = "AllEthosApiConfigurationSettingsBy" + resourceName;

            if (bypassCache && ContainsKey(BuildFullCacheKey(ethosExtensiblityCacheKey)))
            {
                ClearCache(new List<string> { ethosExtensiblityCacheKey });
            }

            var ethosApiConfigurationByResource =
                await GetOrAddToCacheAsync(ethosExtensiblityCacheKey,
                    async () =>
                    {
                        try
                        {
                            EdmSelectCriteria selectCriteria = null;
                            var extendConfigData = (await GetEthosApiConfiguration(resourceName, bypassCache)).ToList();

                            //make sure there is extended config data, if not return null
                            if (extendConfigData == null || !extendConfigData.Any())
                            {
                                return new EthosApiConfiguration();

                            }

                            var apiConfiguration = extendConfigData.FirstOrDefault(ex => ex.EdmeResourceName == resourceName);

                            //make sure there is extended config data, if not return null
                            if (apiConfiguration == null)
                            {
                                return new EthosApiConfiguration();

                            }

                            if (!string.IsNullOrEmpty(apiConfiguration.EdmeSelectCriteria))
                            {
                                selectCriteria = await GetEthosApiSelectCriteria(apiConfiguration.EdmeSelectCriteria, bypassCache);
                            }
                            if (apiConfiguration != null)
                            {
                                var ethosApiConfiguration = new EthosApiConfiguration()
                                {
                                    ResourceName = apiConfiguration.EdmeResourceName,
                                    ProcessId = apiConfiguration.EdmeProcessId,
                                    ColleagueFileNames = apiConfiguration.EdmeResourceEntities,
                                    ColleagueKeyNames = apiConfiguration.EdmeNkElementName,
                                    ParentResourceName = apiConfiguration.EdmeParentApi,
                                    ApiType = apiConfiguration.EdmeType,
                                    PrimaryEntity = apiConfiguration.EdmePrimaryEntity.EndsWith("VALCODES")
                                        && !string.IsNullOrEmpty(apiConfiguration.EdmePrimaryApplication)
                                        && !string.IsNullOrEmpty(apiConfiguration.EdmePrimaryTableName)
                                        ? (await ValidateValcodeTable(apiConfiguration.EdmePrimaryApplication, apiConfiguration.EdmePrimaryTableName))
                                        : apiConfiguration.EdmePrimaryEntity,
                                    PrimaryKeyName = apiConfiguration.EdmePrimaryKey,
                                    SecondaryKeyName = apiConfiguration.EdmeSecondaryKey,
                                    SecondaryKeyPosition = apiConfiguration.EdmeSecondaryKeyPos,
                                    PrimaryApplication = apiConfiguration.EdmePrimaryApplication,
                                    PrimaryTableName = apiConfiguration.EdmePrimaryTableName,
                                    PrimaryGuidSource = apiConfiguration.EdmePrimaryGuidSource,
                                    PrimaryGuidDbType = apiConfiguration.EdmePrimaryGuidDbType,
                                    PrimaryGuidFileName = apiConfiguration.EdmePrimaryGuidFileName.EndsWith("VALCODES")
                                        && !string.IsNullOrEmpty(apiConfiguration.EdmePrimaryApplication)
                                        && !string.IsNullOrEmpty(apiConfiguration.EdmePrimaryTableName)
                                        ? (await ValidateValcodeTable(apiConfiguration.EdmePrimaryApplication, apiConfiguration.EdmePrimaryTableName))
                                        : apiConfiguration.EdmePrimaryGuidFileName,
                                    PageLimit = apiConfiguration.EdmePageLimit,
                                    CurrentUserIdPath = apiConfiguration.EdmeCurrentUserPath,
                                    Description = apiConfiguration.EdmeComments,
                                    ProcessDesc = apiConfiguration.EdmeProcessDesc,
                                    ApiDomain = apiConfiguration.EdmeApiDomain,
                                    ReleaseStatus = apiConfiguration.EdmeApiStatus
                                };
                                ethosApiConfiguration.HttpMethods = new List<EthosApiSupportedMethods>();
                                foreach (var method in apiConfiguration.EdmeMethodsEntityAssociation)
                                {
                                    if (!string.IsNullOrEmpty(method.EdmeHttpMethodsAssocMember))
                                    {
                                        ethosApiConfiguration.HttpMethods.Add(
                                            new EthosApiSupportedMethods(method.EdmeHttpMethodsAssocMember,
                                            method.EdmeHttpPermissionsAssocMember, method.EdmeHttpMethodDescAssocMember, method.EdmeHttpMethodSummaryAssocMember)
                                        );
                                    }
                                }
                                ethosApiConfiguration.PreparedResponses = new List<EthosApiPreparedResponse>();
                                foreach (var response in apiConfiguration.EdmePreparedResponsesEntityAssociation)
                                {
                                    if (!string.IsNullOrEmpty(response.EdmePromptResponseTextAssocMember))
                                    {
                                        ethosApiConfiguration.PreparedResponses.Add(
                                            new EthosApiPreparedResponse(response.EdmePromptResponseTextAssocMember,
                                            response.EdmePromptResponseTitleAssocMember, response.EdmePromptResponseOptsAssocMember,
                                            response.EdmePromptResponseDfltAssocMember)
                                        );
                                    }
                                }
                                if (selectCriteria != null)
                                {
                                    ethosApiConfiguration.SelectFileName = selectCriteria.EdmsSelectFileName;
                                    ethosApiConfiguration.SelectSubroutineName = selectCriteria.EdmsSelectSubroutine;
                                    ethosApiConfiguration.SavingField = selectCriteria.EdmsSavingField;
                                    ethosApiConfiguration.SavingOption = selectCriteria.EdmsSavingOption;
                                    ethosApiConfiguration.SelectColumnName = selectCriteria.EdmsSelectColumnName;
                                    ethosApiConfiguration.SelectRules = selectCriteria.EdmsSelectRules;
                                    ethosApiConfiguration.SelectParagraph = selectCriteria.EdmsSelectParagraph;

                                    ethosApiConfiguration.SelectionCriteria = new List<EthosApiSelectCriteria>();
                                    foreach (var selection in selectCriteria.EdmsSelectEntityAssociation)
                                    {
                                        if (!string.IsNullOrEmpty(selection.EdmsSelectConnectorAssocMember))
                                        {
                                            ethosApiConfiguration.SelectionCriteria.Add(
                                                new EthosApiSelectCriteria(selection.EdmsSelectConnectorAssocMember,
                                                selection.EdmsSelectColumnAssocMember,
                                                selection.EdmsSelectOperAssocMember,
                                                selection.EdmsSelectValueAssocMember)
                                            );
                                        }
                                    }

                                    ethosApiConfiguration.SortColumns = new List<EthosApiSortCriteria>();
                                    foreach (var sort in selectCriteria.EdmsSortEntityAssociation)
                                    {
                                        if (!string.IsNullOrEmpty(sort.EdmsSortColumnsAssocMember))
                                        {
                                            ethosApiConfiguration.SortColumns.Add(
                                                new EthosApiSortCriteria(sort.EdmsSortColumnsAssocMember,
                                                sort.EdmsSortSequenceAssocMember)
                                            );
                                        }
                                    }
                                }
                                else
                                {
                                    ethosApiConfiguration.SelectFileName = apiConfiguration.EdmePrimaryEntity;
                                    ethosApiConfiguration.SelectRules = new List<string>();
                                    ethosApiConfiguration.SelectParagraph = new List<string>();

                                    ethosApiConfiguration.SelectionCriteria = new List<EthosApiSelectCriteria>();
                                    ethosApiConfiguration.SortColumns = new List<EthosApiSortCriteria>();

                                    if (apiConfiguration.EdmePrimaryEntity.EndsWith("VALCODES"))
                                    {
                                        if (!string.IsNullOrEmpty(apiConfiguration.EdmePrimaryTableName) && !string.IsNullOrEmpty(apiConfiguration.EdmePrimaryApplication))
                                        {
                                            ethosApiConfiguration.SelectFileName = await ValidateValcodeTable(apiConfiguration.EdmePrimaryApplication, apiConfiguration.EdmePrimaryTableName);
                                        }
                                        ethosApiConfiguration.SavingField = "VAL.INTERNAL.CODE";
                                        ethosApiConfiguration.SelectionCriteria.Add(
                                            new EthosApiSelectCriteria("WITH", "VALCODE.ID", "EQ", '"' + apiConfiguration.EdmePrimaryTableName + '"')
                                        );
                                        ethosApiConfiguration.SortColumns.Add(
                                            new EthosApiSortCriteria("VAL.INTERNAL.CODE", "BY.EXP")
                                        );
                                    }
                                    else
                                    {
                                        if (!apiConfiguration.EdmePrimaryGuidDbType.Equals("K", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(apiConfiguration.EdmePrimaryGuidSource))
                                        {
                                            ethosApiConfiguration.SelectFileName = !string.IsNullOrEmpty(apiConfiguration.EdmePrimaryGuidFileName) ? apiConfiguration.EdmePrimaryGuidFileName : apiConfiguration.EdmePrimaryEntity;
                                            ethosApiConfiguration.SavingField = apiConfiguration.EdmePrimaryGuidSource;
                                            ethosApiConfiguration.SelectionCriteria.Add(
                                                new EthosApiSelectCriteria("WITH", apiConfiguration.EdmePrimaryGuidSource, "NE", "''")
                                            );

                                            if (!apiConfiguration.EdmePrimaryGuidDbType.Equals("D", StringComparison.OrdinalIgnoreCase) && !apiConfiguration.EdmePrimaryGuidDbType.Equals("X", StringComparison.OrdinalIgnoreCase))
                                                ethosApiConfiguration.SortColumns.Add(
                                                    new EthosApiSortCriteria(apiConfiguration.EdmePrimaryGuidSource, "BY.EXP")
                                            );
                                        }
                                        else
                                        {
                                            if (!string.IsNullOrEmpty(apiConfiguration.EdmePrimaryKey))
                                            {
                                                ethosApiConfiguration.SelectFileName = apiConfiguration.EdmePrimaryEntity;
                                                if (!string.IsNullOrEmpty(apiConfiguration.EdmeSecondaryKey))
                                                {
                                                    ethosApiConfiguration.SelectionCriteria.Add(
                                                        new EthosApiSelectCriteria("WITH", apiConfiguration.EdmeSecondaryKey, "NE", "''")
                                                    );
                                                    ethosApiConfiguration.SortColumns.Add(
                                                        new EthosApiSortCriteria(apiConfiguration.EdmeSecondaryKey, "BY.EXP")
                                                    );
                                                }
                                            }
                                        }
                                    }
                                }

                                if (ethosApiConfiguration != null)
                                {
                                    return ethosApiConfiguration;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Error(e, string.Concat("Retreiving Ethos Extended Confguration has failed for resource : ", resourceName));
                        }

                        //if it makes it here there were no configuration rows or an exception so return null
                        return new EthosApiConfiguration();
                    }, CacheTimeout);

            if (ethosApiConfigurationByResource == null)
            {
                return null;
            }

            return ethosApiConfigurationByResource;

        }

        /// <summary>
        /// Gets the extended data available on a resource, returns an empty list if there are no 
        /// </summary>
        /// <param name="resourceName">name of the resource (api) </param>
        /// <param name="resourceVersionNumber">version number of ther resource</param>
        /// <param name="extendedSchemaResourceId">extended schema identifier</param>
        /// <param name="resourceIds">IEnumerable of the ids for the resources in guid form</param>
        /// <param name="reportEthosApiErrors">Flag to determine if we should throw an exception on Extended Errors.</param>
        /// <param name="bypassCache">Flag to indicate if we should bypass the cache and read directly from disk.</param>
        /// <returns>List with all of the extended data if aavailable. Returns an empty list if none available or none configured</returns>
        public async Task<IEnumerable<EthosExtensibleData>> GetExtendedEthosDataByResource(string resourceName, string resourceVersionNumber,
            string extendedSchemaResourceId, IEnumerable<string> resourceIds, Dictionary<string, Dictionary<string, string>> allColumnData = null,
            bool reportEthosApiErrors = false, bool bypassCache = false, bool useRecordKey = false, bool returnRestrictedFields = false)
        {

            var retConfigData = new List<EthosExtensibleData>();
            var exception = new RepositoryException("Extensibility configuration errors.");

            var extensionConfigData = await GetEthosApiConfiguration(resourceName, bypassCache);
            if (extensionConfigData == null || !extensionConfigData.Any())
            {
                return retConfigData;
            }
            var extendConfigData = (await GetEthosExtensibilityConfiguration(resourceName, bypassCache)).ToList();
            if (extendConfigData == null || !extendConfigData.Any())
            {
                return retConfigData;
            }
            var apiConfiguration = await GetEthosApiConfigurationByResource(resourceName, bypassCache);
            if (apiConfiguration == null)
            {
                return retConfigData;
            }

            var matchingExtensionConfigData = extensionConfigData.FirstOrDefault(e =>
                e.Recordkey.Equals(resourceName, StringComparison.OrdinalIgnoreCase));

            // Look for version specific configuration first
            var matchingExtendedConfigData = extendConfigData.FirstOrDefault(e =>
                e.EdmvResourceName.Equals(resourceName, StringComparison.OrdinalIgnoreCase) &&
                e.EdmvVersionNumber.Equals(resourceVersionNumber, StringComparison.OrdinalIgnoreCase));

            // Look for a major version match.
            if (matchingExtendedConfigData == null)
            {
                if (!string.IsNullOrEmpty(resourceVersionNumber) && resourceVersionNumber.Contains('.'))
                {
                    var majorVersion = resourceVersionNumber.Split('.')[0];
                    var minorVersion = resourceVersionNumber.Split('.')[1];
                    matchingExtendedConfigData = extendConfigData.LastOrDefault(e =>
                        e.EdmvResourceName.Equals(resourceName, StringComparison.OrdinalIgnoreCase) &&
                        e.EdmvVersionNumber.Equals(majorVersion, StringComparison.OrdinalIgnoreCase));

                    if (matchingExtendedConfigData == null)
                    {
                        matchingExtendedConfigData = extendConfigData.LastOrDefault(e =>
                            e.EdmvResourceName.Equals(resourceName, StringComparison.OrdinalIgnoreCase) &&
                            e.EdmvVersionNumber.Contains('.') &&
                            e.EdmvVersionNumber.Split('.')[0].Equals(majorVersion, StringComparison.OrdinalIgnoreCase) &&
                            Convert.ToInt32(e.EdmvVersionNumber.Split('.')[1]) >= Convert.ToInt32(minorVersion));
                    }
                }
            }

            // If we don't have a version specific or major version configuration, then look for versionless match.
            if (matchingExtendedConfigData == null)
            {
                matchingExtendedConfigData = extendConfigData.FirstOrDefault(e =>
                    e.EdmvResourceName.Equals(resourceName, StringComparison.OrdinalIgnoreCase) &&
                    string.IsNullOrEmpty(e.EdmvVersionNumber));
            }

            // TODO: SRM Uncomment when we want to support the metadata Object
            //if (matchingExtendedConfigData == null)
            //{
            //    matchingExtendedConfigData = new EdmExtVersions()
            //    {
            //        EdmvResourceName = resourceName,
            //        EdmvVersionNumber = resourceVersionNumber,
            //        EdmvColumnsEntityAssociation = new List<EdmExtVersionsEdmvColumns>(),
            //        EdmvColumnName = new List<string>(),
            //        EdmvConversion = new List<string>(),
            //        EdmvDatabaseUsageType = new List<string>(),
            //        EdmvDateTimeLink = new List<string>(),
            //        EdmvFileName = new List<string>(),
            //        EdmvJsonLabel = new List<string>(),
            //        EdmvJsonPath = new List<string>(),
            //        EdmvJsonPropertyType = new List<string>(),
            //        EdmvLength = new List<int?>()
            //    };
            //}

            if (resourceIds == null || !resourceIds.Any())
            {
                return retConfigData;
            }

            // TODO: SRM Uncomment when we want to support the metadata Object
            //matchingExtendedConfigData = await AddEthosMetadataConfigurationData(matchingExtendedConfigData, idDict, bypassCache);

            //make sure there is extended config data and row data, if not return empty list
            if (matchingExtendedConfigData == null || matchingExtendedConfigData.EdmvColumnsEntityAssociation == null || !matchingExtendedConfigData.EdmvColumnsEntityAssociation.Any())
            {
                if (reportEthosApiErrors)
                {
                    var message = string.Format("No matching version with valid columns found for resource '{0}' version number '{1}'", resourceName, resourceVersionNumber);
                    logger.Error(message);
                    exception.AddError(new RepositoryError("Bad.Data", message));
                    throw exception;
                }
                return retConfigData;
            }

            GetEthosExtendedDataResponse response = null;
            var colleagueSecondaryKeys = new Dictionary<string, string>();

            // Only execute the transaction if we don't already have the column data coming into the method
            // Data is passed into the method using the service layer property EthosExtendedDataDictionary
            // and used in the BaseCoordinationService to build allColumnData before passing it into this
            // method for processing.  Only business process APIs will pass in this data on POST or PUT requests.
            if (allColumnData == null || !allColumnData.Any() || !matchingExtensionConfigData.EdmeType.Equals("T", StringComparison.OrdinalIgnoreCase))
            {
                allColumnData = new Dictionary<string, Dictionary<string, string>>();
                response = await GetEthosExtendedDataResponse(apiConfiguration, matchingExtendedConfigData, resourceName, resourceVersionNumber,
                    resourceIds, reportEthosApiErrors, bypassCache, useRecordKey, returnRestrictedFields);

                if (response == null || response.ResourceDataObject == null || !response.ResourceDataObject.Any())
                {
                    return retConfigData;
                }
                // Process the Data returned by the GET request (Or POST response in some cases)
                try
                {
                    if (response.Error || (response.GetEthosExtendDataErrors != null && response.GetEthosExtendDataErrors.Any()))
                    {
                        var message = string.Format("Extensibility configuration errors for resource '{0}' version number '{1}'", resourceName, resourceVersionNumber);
                        logger.Error(message);
                        exception.AddError(new RepositoryError("Bad.Data", message));

                        foreach (var error in response.GetEthosExtendDataErrors)
                        {
                            message = string.Concat(error.ErrorCodes, " - ", error.ErrorMessages);
                            logger.Error(message);
                            if (reportEthosApiErrors)
                            {
                                exception.AddError(new RepositoryError("Bad.Data", message));
                            }
                        }
                        if (reportEthosApiErrors && exception != null && exception.Errors != null && exception.Errors.Any())
                        {
                            throw exception;
                        }
                        return retConfigData;
                    }

                    if (response.ResourceDataObject != null && response.ResourceDataObject.Any())
                    {
                        foreach (var resp in response.ResourceDataObject)
                        {
                            if (resp.ColumnNames != null && resp.PropertyValues != null)
                            {
                                var columnNames = resp.ColumnNames.Split(_SM).ToList();
                                var columnValues = resp.PropertyValues.Split(_SM).ToList();
                                Dictionary<string, string> columnDataItem = new Dictionary<string, string>();
                                var index = 0;
                                foreach (var columnName in columnNames)
                                {
                                    try
                                    {
                                        var inputData = columnValues.ElementAt(index).Replace(_TM, _VM).Replace(_XM, _SM);
                                        var outputData = await GetOutputDataHookAsync(columnName, inputData, columnNames, columnValues, resourceName, resourceVersionNumber, reportEthosApiErrors, bypassCache);


                                        if (!columnDataItem.ContainsKey(columnName))
                                        {
                                            if (!string.IsNullOrEmpty(outputData))
                                            {
                                                columnDataItem.Add(columnName, outputData);
                                            }
                                            else
                                            {
                                                columnDataItem.Add(columnName, inputData);
                                            }
                                        }
                                    }
                                    catch (ColleagueSessionExpiredException csee)
                                    {
                                        logger.Error(csee, "Colleague session expired while retrieving extensibility configuration property on a resource");
                                        throw;
                                    }
                                    catch (Exception ex)
                                    {
                                        var message = string.Concat(ex.Message, ": Extensibility configuration property setup error(s). ", resourceName, " version number ", resourceVersionNumber);
                                        logger.Error(message);
                                        exception.AddError(new RepositoryError("Data.Access", message));
                                        continue;
                                    }
                                    index++;
                                }
                                var guidIndex = resp.ResourceGuids;
                                if (!string.IsNullOrEmpty(resp.ResourceSecondaryKeys))
                                {
                                    if (!string.IsNullOrEmpty(apiConfiguration.PrimaryKeyName) && !string.IsNullOrEmpty(apiConfiguration.SecondaryKeyName))
                                    {
                                        guidIndex = EncodePrimaryKey(string.Concat(resp.ResourceFileNames, "+", resp.ResourcePrimaryKeys, "+", resp.ResourceSecondaryKeys));
                                    }
                                    if (!colleagueSecondaryKeys.ContainsKey(guidIndex))
                                    {
                                        colleagueSecondaryKeys.Add(guidIndex, resp.ResourceSecondaryKeys);
                                    }
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(apiConfiguration.PrimaryKeyName))
                                    {
                                        if (!UnEncodePrimaryKey(guidIndex).Contains('+'))
                                        {
                                            guidIndex = EncodePrimaryKey(string.Concat(resp.ResourceFileNames, "+", resp.ResourcePrimaryKeys));
                                        }
                                    }
                                }
                                //if the allColumndata contains the key column data has already been added for this record 
                                //so the new column data must be combined with the existing column data
                                if (allColumnData.ContainsKey(guidIndex))
                                {
                                    var currentColumnDictionary = allColumnData[guidIndex];
                                    var unionOfResults = currentColumnDictionary.Union(columnDataItem)
                                        .ToDictionary(k => k.Key, v => v.Value);
                                    allColumnData[guidIndex] = unionOfResults;
                                }
                                else //key didn't exist yet so just add the guid as the key with the current column data set
                                {
                                    allColumnData.Add(guidIndex, columnDataItem);
                                }
                            }
                        }
                    }
                }
                catch (ColleagueSessionExpiredException csee)
                {
                    logger.Error(csee.Message);
                    throw;
                }
                catch (Exception ex)
                {
                    if (reportEthosApiErrors)
                    {
                        throw ex;
                    }
                    logger.Error(string.Concat(ex.Message));
                    return retConfigData;
                }
            }            

            //list of linked column config data for datetime columns, exlcuding date or time only ones.
            var linkedColumnDetails = matchingExtendedConfigData.EdmvColumnsEntityAssociation
                .Where(l => !string.IsNullOrEmpty(l.EdmvDateTimeLinkAssocMember) &&
                            !l.EdmvJsonPropertyTypeAssocMember.Equals("date", StringComparison.OrdinalIgnoreCase)).ToList();

            foreach (var cData in allColumnData)
            {
                var newEthosThing = new EthosExtensibleData(resourceName, resourceVersionNumber, matchingExtendedConfigData.EdmvExtendedSchemaType, cData.Key, colleagueTimeZone);
                newEthosThing.CurrentUserIdPath = matchingExtensionConfigData.EdmeCurrentUserPath;

                //dictionary to hold linked columns for special processing
                //T1 - data type(date or time), T2 - link value, T3 - column name, T4 - column value 
                var linkedColumnsTuple = new List<Tuple<string, string, string, string>>();

                foreach (var colKeyValuePair in cData.Value.ToList())
                {
                    string colleagueValue = colKeyValuePair.Value;
                    //check if the column returned has a value, if not continue to next item
                    if (string.IsNullOrEmpty(colKeyValuePair.Value))
                    {
                        continue;
                    }
                    var columnConfigDetails = matchingExtendedConfigData.EdmvColumnsEntityAssociation.FirstOrDefault(m =>
                        m.EdmvColumnNameAssocMember.Equals(colKeyValuePair.Key, StringComparison.OrdinalIgnoreCase));
                    //if a column detail isn't matched continue out
                    if (columnConfigDetails == null)
                    {
                        if (apiConfiguration.ApiType == null || string.IsNullOrEmpty(apiConfiguration.ApiType) || !apiConfiguration.ApiType.Equals("T", StringComparison.OrdinalIgnoreCase))
                        {
                            var message = string.Concat("Extensibility configuration does not have a column configuration for ", colKeyValuePair.Key, " for resource ", resourceName);
                            logger.Error(message);
                            //exception.AddError(new RepositoryError("Data.Access", message));
                        }
                        continue;
                    }
                    // Determine the Colleague Data value to be returned with each record key.
                    if (!string.IsNullOrEmpty(columnConfigDetails.EdmvAssociationControllerAssocMember) && (columnConfigDetails.EdmvDatabaseUsageTypeAssocMember.ToUpper() == "A"))
                    {
                        // If we are dealing with a Valcode table, we need to find the guid key and
                        // compare that to the resource key to find the associated value in a multi-valued
                        // associated data element.
                        var associationColumn = matchingExtendedConfigData.EdmvColumnsEntityAssociation.FirstOrDefault(m =>
                            m.EdmvColumnNameAssocMember.Equals(columnConfigDetails.EdmvAssociationControllerAssocMember));

                        var secondaryKeyValue = cData.Value.FirstOrDefault(cdv => cdv.Key == columnConfigDetails.EdmvAssociationControllerAssocMember);
                        //if (!string.IsNullOrEmpty(apiConfiguration.SecondaryKeyName) && columnConfigDetails.EdmvColumnNameAssocMember.Equals(apiConfiguration.SecondaryKeyName, StringComparison.OrdinalIgnoreCase))
                        if (!string.IsNullOrEmpty(apiConfiguration.SecondaryKeyName) && !apiConfiguration.SecondaryKeyName.Equals(columnConfigDetails.EdmvAssociationControllerAssocMember))
                        {
                            // If we are using a primary and secondary key to expose records, then find the colleague value
                            // that matches the secondary key assigned to this record.
                            secondaryKeyValue = cData.Value.FirstOrDefault(cdv => cdv.Key == apiConfiguration.SecondaryKeyName);
                            var tempColumn = matchingExtendedConfigData.EdmvColumnsEntityAssociation.FirstOrDefault(m =>
                             m.EdmvColumnNameAssocMember.Equals(apiConfiguration.SecondaryKeyName));
                            if (tempColumn != null)
                            {
                                associationColumn = tempColumn;
                            }
                        }

                        if (secondaryKeyValue.Key != null && secondaryKeyValue.Value != null && !string.IsNullOrEmpty(secondaryKeyValue.Value))
                        {
                            var secondaryKeyList = secondaryKeyValue.Value.Split(_VM);
                            var colleagueSecondaryKey = colleagueSecondaryKeys.FirstOrDefault(rk => rk.Key == cData.Key);
                            if (colleagueSecondaryKey.Key != null && colleagueSecondaryKey.Value != null && !string.IsNullOrEmpty(colleagueSecondaryKey.Value))
                            {
                                var secondaryKey = colleagueSecondaryKey.Value;
                                if (secondaryKeyList != null && secondaryKey != null)
                                {
                                    for (var i = 0; i < secondaryKeyList.Count(); i++)
                                    {
                                        var secondaryKeyListValue = await ConvertExtendedValueToColleagueValue(associationColumn, secondaryKeyList[i]);
                                        if (associationColumn != null && associationColumn.EdmvJsonPropertyTypeAssocMember.StartsWith("date", StringComparison.OrdinalIgnoreCase))
                                        {
                                            try
                                            {
                                                var dateValues = secondaryKeyListValue.Split('-');
                                                if (dateValues.Count() >= 3)
                                                {
                                                    var year = Convert.ToInt32(dateValues[0]);
                                                    var month = Convert.ToInt32(dateValues[1]);
                                                    var day = Convert.ToInt32(dateValues[2]);
                                                    secondaryKeyListValue = DmiString.DateTimeToPickDate(new DateTime(year, month, day)).ToString();
                                                }
                                            }
                                            catch (ColleagueSessionExpiredException csee)
                                            {
                                                logger.Error(csee, "Colleague session expired while processing date value.");
                                                throw;
                                            }
                                            catch (Exception ex)
                                            {
                                                // Ignore for now.
                                                logger.Error(ex.Message, "Cannot process date value.");
                                            }
                                        }
                                        if (secondaryKeyListValue == secondaryKey)
                                        {
                                            var newValue = colKeyValuePair.Value.Split(_VM);
                                            if (i < newValue.Count())
                                            {
                                                colleagueValue = newValue[i];
                                            }
                                            else colleagueValue = string.Empty;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        // If we have comments or text, then convert value mark or sub-value marks to spaces.
                        if (!columnConfigDetails.EdmvJsonLabelAssocMember.EndsWith("[]"))
                        {
                            if (columnConfigDetails.EdmvDatabaseUsageTypeAssocMember.Equals("C", StringComparison.OrdinalIgnoreCase) || columnConfigDetails.EdmvDatabaseUsageTypeAssocMember.Equals("T", StringComparison.OrdinalIgnoreCase))
                            {
                                var secondaryUsageType = columnConfigDetails.EdmvSecDatabaseUsageTypeAssocMember.ToUpper();
                                if (secondaryUsageType == "Q" || secondaryUsageType == "A" || secondaryUsageType == "L")
                                {
                                    colleagueValue = colKeyValuePair.Value.Replace(_SM, ' ');
                                }
                                else
                                {
                                    colleagueValue = colKeyValuePair.Value.Replace(_VM, ' ');
                                }
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(apiConfiguration.SecondaryKeyName) && columnConfigDetails.EdmvColumnNameAssocMember.Equals(apiConfiguration.SecondaryKeyName, StringComparison.OrdinalIgnoreCase))
                                {
                                    // If we are using a primary and secondary key to expose records, then find the colleague value
                                    // that matches the secondary key assigned to this record.
                                    var secondaryKeyValue = colKeyValuePair;
                                    if (secondaryKeyValue.Key != null && secondaryKeyValue.Value != null && !string.IsNullOrEmpty(secondaryKeyValue.Value))
                                    {
                                        var secondaryKeyList = secondaryKeyValue.Value.Split(_VM);
                                        var colleagueSecondaryKey = colleagueSecondaryKeys.FirstOrDefault(rk => rk.Key == cData.Key);
                                        if (colleagueSecondaryKey.Key != null && colleagueSecondaryKey.Value != null && !string.IsNullOrEmpty(colleagueSecondaryKey.Value))
                                        {
                                            var secondaryKey = colleagueSecondaryKey.Value;
                                            if (secondaryKeyList != null && secondaryKey != null)
                                            {
                                                for (var i = 0; i < secondaryKeyList.Count(); i++)
                                                {
                                                    var secondaryKeyListValue = secondaryKeyList[i];
                                                    if (columnConfigDetails != null && columnConfigDetails.EdmvJsonPropertyTypeAssocMember.StartsWith("date", StringComparison.OrdinalIgnoreCase))
                                                    {
                                                        try
                                                        {
                                                            var dateValues = secondaryKeyListValue.Split('-');
                                                            if (dateValues.Count() >= 3)
                                                            {
                                                                var year = Convert.ToInt32(dateValues[0]);
                                                                var month = Convert.ToInt32(dateValues[1]);
                                                                var day = Convert.ToInt32(dateValues[2]);
                                                                secondaryKeyListValue = DmiString.DateTimeToPickDate(new DateTime(year, month, day)).ToString();
                                                            }
                                                        }
                                                        catch (ColleagueSessionExpiredException csee)
                                                        {
                                                            logger.Error(csee, "Colleague session expired while processing date value.");
                                                            throw;
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            // Ignore for now.
                                                            logger.Error(ex.Message, "Cannot process date value.");
                                                        }
                                                    }
                                                    if (secondaryKeyListValue == secondaryKey)
                                                    {
                                                        var newValue = colKeyValuePair.Value.Split(_VM);
                                                        if (i < newValue.Count())
                                                        {
                                                            colleagueValue = newValue[i];
                                                        }
                                                        else colleagueValue = string.Empty;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (string.IsNullOrEmpty(colleagueValue))
                    {
                        continue;
                    }
                    if (columnConfigDetails.EdmvDatabaseUsageTypeAssocMember.Equals("K", StringComparison.OrdinalIgnoreCase) && colleagueValue.Contains("*"))
                    {
                        int counter = 1;
                        if (!string.IsNullOrEmpty(matchingExtensionConfigData.EdmeSecondaryKey)) counter++;
                        if (matchingExtensionConfigData == null || matchingExtensionConfigData.EdmeNewKeyStrategyEntityAssociation == null || matchingExtensionConfigData.EdmeNewKeyStrategyEntityAssociation.Count() > counter)
                        {
                            var collIdValue = colleagueValue.Split('*');
                            var index = columnConfigDetails.EdmvFieldNumberAssocMember;
                            if (index != null && index > 0)
                            {
                                if (collIdValue.Count() >= index)
                                {
                                    colleagueValue = collIdValue[(int)index - 1];
                                }
                            }
                        }
                    }

                    // if this is a match to an item i the linkedColumnDetail this means it is a linked column and needs to be treated different
                    if (linkedColumnDetails.Any(c => c.EdmvColumnNameAssocMember.Equals(colKeyValuePair.Key, StringComparison.OrdinalIgnoreCase)))
                    {
                        var jsonPropType = columnConfigDetails.EdmvConversionAssocMember.StartsWith("D", StringComparison.OrdinalIgnoreCase)
                            ? "date" : "time";

                        linkedColumnsTuple.Add(new Tuple<string, string, string, string>(jsonPropType,
                            columnConfigDetails.EdmvDateTimeLinkAssocMember, colKeyValuePair.Key,
                            colleagueValue));
                    }
                    else 
                    {
                        // If this is a row for a key field in the list of key properties then create a new
                        // "id" row for this property.
                        if (!string.IsNullOrEmpty(apiConfiguration.ApiType) && apiConfiguration.ApiType.Equals("T", StringComparison.OrdinalIgnoreCase) && apiConfiguration.ColleagueKeyNames != null && apiConfiguration.ColleagueKeyNames.Contains(columnConfigDetails.EdmvColumnNameAssocMember))
                        {
                            string jsonPath = !string.IsNullOrEmpty(columnConfigDetails.EdmvJsonPathAssocMember) ? columnConfigDetails.EdmvJsonPathAssocMember : "/";
                            string propertyName = columnConfigDetails.EdmvJsonLabelAssocMember;
                            if (!jsonPath.StartsWith("/id/") || columnConfigDetails.EdmvJsonLabelAssocMember.Equals("id", StringComparison.OrdinalIgnoreCase))
                            {
                                if (apiConfiguration.ColleagueKeyNames.Count() == 1)
                                {
                                    // Setup ID property to contain a single string/id value
                                    jsonPath = "/";
                                    propertyName = "id";
                                }
                                else
                                {
                                    // Setup ID object to contain multiple property names.
                                    jsonPath = string.Concat("/id", jsonPath);
                                }
                                var idRow = new EthosExtensibleDataRow(columnConfigDetails.EdmvColumnNameAssocMember,
                                    columnConfigDetails.EdmvFileNameAssocMember, propertyName, jsonPath,
                                    columnConfigDetails.EdmvJsonPropertyTypeAssocMember, colleagueValue)
                                {
                                    AssociationController = columnConfigDetails.EdmvAssociationControllerAssocMember,
                                    TransType = columnConfigDetails.EdmvTransTypeAssocMember,
                                    DatabaseUsageType = columnConfigDetails.EdmvDatabaseUsageTypeAssocMember,
                                    Description = columnConfigDetails.EdmvColumnDescAssocMember

                                };
                                newEthosThing.AddItemToExtendedData(idRow);

                                if (apiConfiguration.ColleagueKeyNames.Count() == 1)
                                {
                                    // Add original row to return values
                                    var row = new EthosExtensibleDataRow(columnConfigDetails.EdmvColumnNameAssocMember, columnConfigDetails.EdmvFileNameAssocMember,
                                    columnConfigDetails.EdmvJsonLabelAssocMember, columnConfigDetails.EdmvJsonPathAssocMember,
                                    columnConfigDetails.EdmvJsonPropertyTypeAssocMember, colleagueValue)
                                    {
                                        AssociationController = columnConfigDetails.EdmvAssociationControllerAssocMember,
                                        TransType = columnConfigDetails.EdmvTransTypeAssocMember,
                                        DatabaseUsageType = columnConfigDetails.EdmvDatabaseUsageTypeAssocMember,
                                        Description = columnConfigDetails.EdmvColumnDescAssocMember

                                    };
                                    newEthosThing.AddItemToExtendedData(row);
                                }
                            }
                            else
                            {
                                var row = new EthosExtensibleDataRow(columnConfigDetails.EdmvColumnNameAssocMember, columnConfigDetails.EdmvFileNameAssocMember,
                                    columnConfigDetails.EdmvJsonLabelAssocMember, columnConfigDetails.EdmvJsonPathAssocMember,
                                    columnConfigDetails.EdmvJsonPropertyTypeAssocMember, colleagueValue)
                                {
                                    AssociationController = columnConfigDetails.EdmvAssociationControllerAssocMember,
                                    TransType = columnConfigDetails.EdmvTransTypeAssocMember,
                                    DatabaseUsageType = columnConfigDetails.EdmvDatabaseUsageTypeAssocMember,
                                    Description = columnConfigDetails.EdmvColumnDescAssocMember

                                };
                                newEthosThing.AddItemToExtendedData(row);
                            }
                        }

                        // else this is just a single value column
                        // if (!columnConfigDetails.EdmvJsonPathAssocMember.StartsWith("/id/") && !(columnConfigDetails.EdmvJsonPathAssocMember.Equals("/", StringComparison.OrdinalIgnoreCase) && columnConfigDetails.EdmvJsonLabelAssocMember.Equals("id", StringComparison.OrdinalIgnoreCase)))
                        else
                        {
                            var row = new EthosExtensibleDataRow(columnConfigDetails.EdmvColumnNameAssocMember, columnConfigDetails.EdmvFileNameAssocMember,
                                columnConfigDetails.EdmvJsonLabelAssocMember, columnConfigDetails.EdmvJsonPathAssocMember,
                                columnConfigDetails.EdmvJsonPropertyTypeAssocMember, colleagueValue)
                            {
                                AssociationController = columnConfigDetails.EdmvAssociationControllerAssocMember,
                                TransType = columnConfigDetails.EdmvTransTypeAssocMember,
                                DatabaseUsageType = columnConfigDetails.EdmvDatabaseUsageTypeAssocMember,
                                Description = columnConfigDetails.EdmvColumnDescAssocMember

                            };
                            newEthosThing.AddItemToExtendedData(row);
                        }
                    }
                }
                try
                {
                    var datetimeExtensions = ConvertColleageDateAndTimeValues(linkedColumnsTuple, linkedColumnDetails);

                    datetimeExtensions.ForEach(e => newEthosThing.AddItemToExtendedData(e));
                }
                catch (ColleagueSessionExpiredException csee)
                {
                    logger.Error(csee, "Colleague session expired while getting dateTime for resource id ", cData.Key);
                    throw;
                }
                catch (Exception ex)
                {
                    logger.Error(ex, string.Concat("DateTime get failed for resource id ", cData.Key));
                    var message = string.Concat(ex.Message, " DateTime get failed for resource id '", cData.Key, "'.");
                    exception.AddError(new RepositoryError("Data.Access", message));
                }

                var variableCalcuations = await GetVariableCalculationHookAsync(resourceName, resourceVersionNumber, newEthosThing.ExtendedDataList, reportEthosApiErrors, bypassCache);
                if (variableCalcuations != null && variableCalcuations.Any())
                {
                    variableCalcuations.ForEach(e => newEthosThing.AddItemToExtendedData(e));
                }

                retConfigData.Add(newEthosThing);
            }

            if (reportEthosApiErrors && exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }

            return retConfigData;
        }

        /// <summary>
        /// Execute the proper CTX to get Extended Data from Colleague
        /// </summary>
        /// <param name="apiConfiguration"></param>
        /// <param name="matchingExtendedConfigData"></param>
        /// <param name="resourceName"></param>
        /// <param name="resourceVersionNumber"></param>
        /// <param name="resourceIds"></param>
        /// <param name="reportEthosApiErrors"></param>
        /// <param name="bypassCache"></param>
        /// <param name="useRecordKey"></param>
        /// <returns>Returns the proper response from the CTX call in a single formatted response.</returns>
        private async Task<GetEthosExtendedDataResponse> GetEthosExtendedDataResponse(EthosApiConfiguration apiConfiguration, EdmExtVersions matchingExtendedConfigData,
            string resourceName, string resourceVersionNumber, IEnumerable<string> resourceIds, bool reportEthosApiErrors, bool bypassCache, bool useRecordKey, bool returnRestrictedFields)
        {
            var exception = new RepositoryException("Extensibility configuration errors.");
            GetEthosExtendedDataResponse response = null;
            if (apiConfiguration.ApiType != null && apiConfiguration.ApiType.Equals("T", StringComparison.OrdinalIgnoreCase))
            {
                bool isFileSuite = !string.IsNullOrEmpty(apiConfiguration.SelectFileName) ? await IsEthosFileSuiteTemplateFile(apiConfiguration.SelectFileName, bypassCache) : false;
                var request = new ProcessScreenApiRequest()
                {
                    ProcessId = apiConfiguration.ProcessId,
                    ProcessMode = "GET",
                    KeyData = new List<KeyData>(),
                    ColumnData = new List<ColumnData>()
                };

                foreach (var resourceKey in resourceIds)
                {
                    var splitKey = UnEncodePrimaryKey(resourceKey).Split(_XM);
                    if (splitKey.Count() > 0)
                    {
                        string fieldNames = string.Empty;
                        string fieldValues = string.Empty;
                        string fileNames = string.Empty;
                        string fileValues = string.Empty;

                        for (int i = 0; i < splitKey.Count(); i++)
                        {
                            if (!string.IsNullOrEmpty(splitKey[i]))
                            {
                                var keyPart = splitKey[i];
                                if (keyPart.Contains("+"))
                                {
                                    var idSplit = keyPart.Split('+');
                                    var fldName = idSplit[0];
                                    var fldValue = idSplit[1];
                                    if (matchingExtendedConfigData.EdmvColumnName.Contains(fldName))
                                    {
                                        if (!string.IsNullOrEmpty(fldName) && !string.IsNullOrEmpty(fldValue))
                                        {
                                            if (!string.IsNullOrEmpty(fieldNames))
                                            {
                                                fieldNames = string.Concat(fieldNames, _SM);
                                                fieldValues = string.Concat(fieldValues, _SM);
                                            }
                                            fieldNames = string.Concat(fieldNames, fldName);
                                            fieldValues = string.Concat(fieldValues, fldValue);
                                        }
                                    }
                                    else
                                    {
                                        // Add the file name and key to the list of file names and keys
                                        if (!string.IsNullOrEmpty(fileNames))
                                        {
                                            fileNames = string.Concat(fileNames, _SM);
                                            fileValues = string.Concat(fileValues, _SM);
                                        }
                                        fileNames = string.Concat(fileNames, fldName);
                                        fileValues = string.Concat(fileValues, fldValue);

                                        // Add a column for the primary key value
                                        var keyColumnAssociation = matchingExtendedConfigData.EdmvColumnsEntityAssociation.Where(edc => edc.EdmvFileNameAssocMember == fldName && edc.EdmvDatabaseUsageTypeAssocMember == "K");
                                        if (keyColumnAssociation != null)
                                        {
                                            foreach (var keyAssoc in keyColumnAssociation)
                                            {
                                                var fieldName = keyAssoc.EdmvColumnNameAssocMember;
                                                if (!string.IsNullOrEmpty(fieldName))
                                                {
                                                    if (!string.IsNullOrEmpty(fieldNames))
                                                    {
                                                        fieldNames = string.Concat(fieldNames, _SM);
                                                        fieldValues = string.Concat(fieldValues, _SM);
                                                    }
                                                    fieldNames = string.Concat(fieldNames, fieldName);
                                                    fieldValues = string.Concat(fieldValues, fldValue);
                                                }
                                            }
                                        }
                                        
                                        // If we are working with a file suite, then add the CDD name and value
                                        // to the list of column names and column values.
                                        if (isFileSuite && fldName.Contains("."))
                                        {
                                            var faYear = fldName.Split('.')[1];
                                            var faName = "FA.YEAR";
                                            var glName = "FISCAL.YEAR";
                                            if (!string.IsNullOrEmpty(fieldNames))
                                            {
                                                fieldNames = string.Concat(fieldNames, _SM);
                                                fieldValues = string.Concat(fieldValues, _SM);
                                            }

                                            // Add FA.YEAR and FISCAL.YEAR to list of data elements
                                            fieldNames = string.Concat(fieldNames, faName, _SM, glName);
                                            fieldValues = string.Concat(fieldValues, faYear, _SM, faYear);
                                        }
                                    }
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(fileNames))
                                    {
                                        fileNames = string.Concat(fileNames, _SM);
                                        fileValues = string.Concat(fileValues, _SM);
                                    }
                                    fileNames = string.Concat(fileNames, apiConfiguration.PrimaryEntity);
                                    fileValues = string.Concat(fileValues, keyPart);
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(fieldNames) && !string.IsNullOrEmpty(fieldValues))
                        {
                            request.ColumnData.Add(new ColumnData()
                            {
                                ColumnNames = fieldNames,
                                ColumnValues = fieldValues
                            });
                        }
                        if (!string.IsNullOrEmpty(fileNames) && !string.IsNullOrEmpty(fileValues))
                        {
                            request.KeyData.Add(new KeyData()
                            {
                                PrimaryKeyNames = fileNames,
                                PrimaryKeyValues = fileValues
                            });
                        }
                    }

                    request.CallChain = new List<string>();
                    foreach (var column in matchingExtendedConfigData.EdmvColumnName)
                    {
                        if (column.Contains(':'))
                        {
                            var splitColumns = column.Split(':');
                            for (int i = 0; i < splitColumns.Count() - 1; i++)
                            {
                                if (splitColumns[0].Contains('*'))
                                {
                                    request.CallChain.Add(splitColumns[i].Split('*')[1]);
                                }
                                else
                                {
                                    request.CallChain.Add(splitColumns[i]);
                                }
                            }
                        }
                    }
                    request.CallChain = request.CallChain.Distinct().ToList();
                }

                // Update the prepared Responses from the Configuration for GET request
                request.PreparedResponses = new List<PreparedResponses>();
                if (apiConfiguration.PreparedResponses != null)
                {
                    foreach (var prpr in apiConfiguration.PreparedResponses)
                    {
                        if (!string.IsNullOrEmpty(prpr.DefaultOption))
                        {
                            request.PreparedResponses.Add(new PreparedResponses()
                            {
                                PreparedResponsePrompts = prpr.Text,
                                PreparedResponseOptions = prpr.Options,
                                PreparedResponseValues = prpr.DefaultOption
                            });
                        }
                    }
                }

                // Update request for Restricted Fields to be returned from the UI form process
                request.ReturnSecureData = returnRestrictedFields;

                var getResponse = await transactionInvoker.ExecuteAsync<ProcessScreenApiRequest, ProcessScreenApiResponse>(request);

                if (getResponse.Error || (getResponse.ProcessScreenApiErrors != null && getResponse.ProcessScreenApiErrors.Any()))
                {
                    exception = new RepositoryException();
                    foreach (var error in getResponse.ProcessScreenApiErrors)
                    {
                        var message = string.Concat(error.ErrorCodes, " - ", error.ErrorMessages);
                        logger.Error(message);
                        if (reportEthosApiErrors)
                        {
                            var guid = EncodePrimaryKey(error.ErrorKeys);
                            var sourceId = !string.IsNullOrEmpty(error.ErrorKeys) && error.ErrorKeys.Contains('+') ? error.ErrorKeys.Split('+')[1] : error.ErrorKeys;
                            if (string.IsNullOrEmpty(sourceId) || sourceId.ToUpper().Contains("$NEW"))
                            {
                                guid = string.Empty;
                                sourceId = string.Empty;
                            }
                            exception.AddError(new RepositoryError(error.ErrorCodes, error.ErrorMessages)
                            {
                                Id = guid,
                                SourceId = sourceId
                            });
                        }
                    }
                    if (reportEthosApiErrors && exception != null && exception.Errors != null && exception.Errors.Any())
                    {
                        throw exception;
                    }

                    return response;
                }

                // Set variables needed to respond with X-Content-Restricted of "partial"
                if (getResponse.ReturnSecureData)
                {
                    SecureDataDefinition = new Tuple<bool, List<string>, List<string>>(getResponse.ReturnSecureData, getResponse.DeniedFieldsList, getResponse.SecureFieldsList);
                }

                // Convert response to a GetEthosExtendedDataResponse
                response = new GetEthosExtendedDataResponse()
                {
                    Error = getResponse.Error,
                    GetEthosExtendDataErrors = new List<GetEthosExtendDataErrors>(),
                    ResourceDataObject = new List<ResourceDataObject>()
                };
                foreach (var error in getResponse.ProcessScreenApiErrors)
                {
                    response.GetEthosExtendDataErrors.Add(new GetEthosExtendDataErrors()
                    {
                        ErrorCodes = error.ErrorCodes,
                        ErrorMessages = error.ErrorMessages
                    });
                }
                int idx = 0;
                foreach (var dataObjects in getResponse.ColumnData)
                {
                    if (resourceIds.Count() > idx)
                    {
                        var guidIndex = resourceIds.ElementAt(idx);
                        if (!UnEncodePrimaryKey(guidIndex).Contains('+'))
                        {
                            guidIndex = EncodePrimaryKey(string.Concat(apiConfiguration.PrimaryEntity, "+", guidIndex));
                        }
                        response.ResourceDataObject.Add(new ResourceDataObject()
                        {
                            ColumnNames = dataObjects.ColumnNames,
                            PropertyValues = dataObjects.ColumnValues,
                            ResourceGuids = guidIndex
                        });
                    }
                    idx++;
                }
            }
            else
            {
                var request = new GetEthosExtendedDataRequest()
                {
                    Guids = resourceIds.ToList(),
                    ResourceName = resourceName,
                    Version = resourceVersionNumber,
                    BypassCache = bypassCache

                };

                if (apiConfiguration != null && apiConfiguration.ApiType.Equals("S", StringComparison.OrdinalIgnoreCase))
                {
                    var extendedDataTuple = GetEthosExtendedDataLists();
                    string columnNames = string.Empty;
                    foreach (var name in extendedDataTuple.Item1)
                    {
                        if (!string.IsNullOrEmpty(columnNames)) columnNames = string.Concat(columnNames, _SM);
                        columnNames = string.Concat(columnNames, name);
                    }

                    string columnValues = string.Empty;
                    foreach (var name in extendedDataTuple.Item2)
                    {
                        if (!string.IsNullOrEmpty(columnValues)) columnValues = string.Concat(columnValues, _SM);
                        columnValues = string.Concat(columnValues, name.Replace(_VM, _TM).Replace(_SM, _XM));
                    }

                    var resourceDataObject = new List<ResourceDataObject>()
                    {
                        new ResourceDataObject() { ColumnNames = columnNames, PropertyValues = columnValues }
                    };

                    request.ResourceDataObject = resourceDataObject;
                }
                else
                {
                    if (apiConfiguration != null && (!string.IsNullOrEmpty(apiConfiguration.PrimaryKeyName) || useRecordKey))
                    {
                        request.Guids = new List<string>();
                        var resourceDataObject = new List<ResourceDataObject>();
                        foreach (var id in resourceIds)
                        {
                            var recordKey = UnEncodePrimaryKey(id);
                            if (recordKey.Contains("+"))
                            {
                                var idSplit = recordKey.Split('+');
                                var dataObject = new ResourceDataObject()
                                {
                                    ResourceFileNames = idSplit[0],
                                    ResourcePrimaryKeys = idSplit[1],
                                    ResourceGuids = idSplit[1],
                                    ResourceSecondaryKeys = (idSplit.Count() > 2) ? idSplit[2] : string.Empty,
                                    ResourceSecondaryKeysField = apiConfiguration.SecondaryKeyName
                                };
                                resourceDataObject.Add(dataObject);
                            }
                            else
                            {
                                var dataObject = new ResourceDataObject()
                                {
                                    ResourcePrimaryKeys = recordKey,
                                    ResourceGuids = recordKey,
                                    ResourceFileNames = apiConfiguration.PrimaryEntity
                                };
                                resourceDataObject.Add(dataObject);
                            }
                        }
                        request.ResourceDataObject = resourceDataObject;
                    }
                }
                response = await transactionInvoker.ExecuteAsync<GetEthosExtendedDataRequest, GetEthosExtendedDataResponse>(request);
            }

            if (reportEthosApiErrors && exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }

            return response;
        }

        /// <summary>
        /// Returns a list of all File Suite Templates
        /// </summary>
        /// <param name="bypassCache">bool to determine if cache should be bypassed</param>
        /// <returns>List of data contracts for Templates</returns>
        private async Task<List<FileSuiteTemplates>> GetEthosFileSuiteTemplatesAsync(bool bypassCache = false)
        {
            const string ethosFileSuiteCacheKey = "AllEthosExtendFileSuites";

            if (bypassCache && ContainsKey(BuildFullCacheKey(ethosFileSuiteCacheKey)))
            {
                ClearCache(new List<string> { ethosFileSuiteCacheKey });
            }
            return await GetOrAddToCacheAsync<List<FileSuiteTemplates>>(ethosFileSuiteCacheKey,
                async () =>
                {
                    var ethosFaFileSuitesList =
                        await DataReader.ReadRecordAsync<FileSuiteTemplates>("ST.PARMS", "FST_FA.FILE.SUITES");
                    // Hack in the FA.YEARLY.USER.ACYR and FA.TERM.USER.ACYR file suites
                    if (ethosFaFileSuitesList.FstTemplate.Any() && ethosFaFileSuitesList.FstFilePrefix.Any())
                    {
                        ethosFaFileSuitesList.FstTemplate.Add("FA.YEARLY.USER.ACYR");
                        ethosFaFileSuitesList.FstTemplate.Add("FA.TERM.USER.ACYR");
                        ethosFaFileSuitesList.FstTemplate.Add("FA.YEARLY.USER");
                        ethosFaFileSuitesList.FstTemplate.Add("FA.TERM.USER");
                        ethosFaFileSuitesList.FstTemplate.Add("");
                        ethosFaFileSuitesList.FstTemplate.Add("");
                    }

                    var fsKeys = new List<string>()
                    {
                        "FST_BCT",
                        "FST_BJT",
                        "FST_BOC",
                        "FST_BWK",
                        "FST_BPJ",
                        "FST_GL.SUITES.WITH.GLU"
                    };
                    var ethosCfFileSuitesList =
                        await DataReader.BulkReadRecordAsync<FileSuiteTemplates>("CF.PARMS", fsKeys.ToArray());

                    var fileSuitesList = new List<FileSuiteTemplates>();
                    fileSuitesList.Add(ethosFaFileSuitesList);
                    fileSuitesList.AddRange(ethosCfFileSuitesList);
                    return fileSuitesList;
                }, CacheTimeout);
        }

        /// <summary>
        /// Checks to see if a file name is a template file for file suites.
        /// </summary>
        /// <param name="fileName">Name of the file to check to see if it's a file suite template.</param>
        /// <param name="bypassCache">bool to determine if cache should be bypassed</param>
        /// <returns>True if File Suite Template file</returns>
        private async Task<bool> IsEthosFileSuiteTemplateFile(string fileName, bool bypassCache = false)
        {
            var ethosFileSuitesList = await GetEthosFileSuiteTemplatesAsync(bypassCache);

            foreach (var template in ethosFileSuitesList)
            {
                foreach (var templateFile in template.FstmpltEntityAssociation)
                {
                    if (templateFile.FstTemplateAssocMember == fileName)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Checks to see if a file name is an instance of a file suite.
        /// </summary>
        /// <param name="fileName">Name of the file to check to see if it's a file suite instance.</param>
        /// <param name="bypassCache">bool to determine if cache should be bypassed</param>
        /// <returns>Returns file suite instance, such as, 2001, 2001, etc. (not file name).</returns>
        private async Task<string> GetEthosFileSuiteInstance(string fileName, bool bypassCache = false)
        {
            var ethosFileSuitesList = await GetEthosFileSuiteTemplatesAsync(bypassCache);

            foreach (var template in ethosFileSuitesList)
            {
                foreach (var templateFile in template.FstmpltEntityAssociation)
                {
                    var prefix = templateFile.FstFilePrefixAssocMember;
                    var suffix = templateFile.FstFileSuffixAssocMember;
                    if (!string.IsNullOrEmpty(prefix) && fileName.Contains(prefix))
                    {
                        var match = true;
                        var fullFileNameSplit = fileName.Split('.');
                        var prefixSplit = prefix.Split('.');
                        for (var i = 0; i < prefixSplit.Count() && i < fullFileNameSplit.Count(); i++)
                        {
                            if (prefixSplit[i] != fullFileNameSplit[i]) match = false;
                        }
                        if (!string.IsNullOrEmpty(suffix))
                        {
                            var suffixSplit = suffix.Split('.');
                            var k = fullFileNameSplit.Count();
                            for (var i = suffixSplit.Count(); i > 0 && k > 0; i--)
                            {
                                if (prefixSplit[i - 1] != fullFileNameSplit[k - 1]) match = false;
                                k--;
                            }
                        }
                        if (match) return fullFileNameSplit[prefixSplit.Count()];
                    }
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Returns file suite template if a file name is an instance of a file suite.
        /// </summary>
        /// <param name="fileName">Name of the file to check to see if it's a file suite template.</param>
        /// <param name="bypassCache">bool to determine if cache should be bypassed</param>
        /// <returns>File Suite template or file name of non-template file.</returns>
        private async Task<string> GetEthosFileSuiteTemplate(string fileName, bool bypassCache = false)
        {
            var ethosFileSuitesList = await GetEthosFileSuiteTemplatesAsync(bypassCache);

            foreach (var template in ethosFileSuitesList)
            {
                foreach (var templateFile in template.FstmpltEntityAssociation)
                {
                    var prefix = templateFile.FstFilePrefixAssocMember;
                    var suffix = templateFile.FstFileSuffixAssocMember;
                    if (!string.IsNullOrEmpty(prefix) && fileName.Contains(prefix))
                    {
                        var match = true;
                        var fullFileNameSplit = fileName.Split('.');
                        var prefixSplit = prefix.Split('.');
                        for (var i = 0; i < prefixSplit.Count() && i < fullFileNameSplit.Count(); i++)
                        {
                            if (prefixSplit[i] != fullFileNameSplit[i]) match = false;
                        }
                        if (!string.IsNullOrEmpty(suffix))
                        {
                            var suffixSplit = suffix.Split('.');
                            var k = fullFileNameSplit.Count();
                            for (var i = suffixSplit.Count(); i > 0 && k > 0; i--)
                            {
                                if (prefixSplit[i - 1] != fullFileNameSplit[k - 1]) match = false;
                                k--;
                            }
                        }
                        if (match) return templateFile.FstTemplateAssocMember;
                    }
                }
            }
            return fileName;
        }

        private async Task<string> GetEthosFileSuiteFileNameAsync(string fileName, string instance, bool bypassCache = false)
        {
            var ethosFileSuitesList = await GetEthosFileSuiteTemplatesAsync(bypassCache);

            foreach (var template in ethosFileSuitesList)
            {
                foreach (var templateFile in template.FstmpltEntityAssociation)
                {
                    if (templateFile.FstTemplateAssocMember == fileName)
                    {
                        var prefix = templateFile.FstFilePrefixAssocMember;
                        var suffix = templateFile.FstFileSuffixAssocMember;
                        var physicalFileName = string.Concat(prefix, ".", instance);
                        if (!string.IsNullOrEmpty(suffix))
                        {
                            physicalFileName = string.Concat(physicalFileName, ".", suffix);
                        }
                        return physicalFileName;
                    }
                }
            }
            return string.Empty;
        }

        private IEnumerable<EthosExtensibleDataRow> ConvertColleageDateAndTimeValues(IEnumerable<Tuple<string, string, string, string>> valuesTuples, IEnumerable<EdmExtVersionsEdmvColumns> columnConfigs)
        {
            //T1 - data type(date or time), T2 - link value, T3 - column name, T4 - column value

            var returnList = new List<EthosExtensibleDataRow>();

            var distinctLinks = valuesTuples.Select(e => e.Item2).ToList().Distinct();

            foreach (var link in distinctLinks)
            {
                //get date tuple for this link
                var dateTuple = valuesTuples.FirstOrDefault(e =>
                    e.Item2.Equals(link) && e.Item1.Equals("date", StringComparison.OrdinalIgnoreCase));

                //get time tuple for this link
                var timeTuple = valuesTuples.FirstOrDefault(e =>
                    e.Item2.Equals(link) && e.Item1.Equals("time", StringComparison.OrdinalIgnoreCase));

                //if either are null
                //if (dateTuple == null || timeTuple == null)
                if (dateTuple == null)
                {
                    continue;
                }

                //get column config info for date column, this will be used to create the EthosExtensibleDataRow return
                var dateColumnConfig = columnConfigs.FirstOrDefault(m =>
                    m.EdmvColumnNameAssocMember.Equals(dateTuple.Item3, StringComparison.OrdinalIgnoreCase));
                //if the column config can't be found don't try and add
                if (dateColumnConfig == null)
                {
                    continue;
                }

                var dateValues = dateTuple.Item4.Split(_VM);
                string[] timeValues;
                if (timeTuple != null)
                {
                    timeValues = timeTuple.Item4.Split(_VM);
                }
                else
                {
                    timeValues = string.Empty.Split(_VM);
                }

                var maxDateTime = dateValues.Count();
                if (timeValues.Count() > maxDateTime)
                    maxDateTime = timeValues.Count();
                var utcDateTimeStringValues = string.Empty;

                for (int valIdx = 0; valIdx < maxDateTime; valIdx++)
                {
                    int intDateValue, intTimeValue;

                    var dateValue = valIdx < dateValues.Count() ? dateValues[valIdx] : string.Empty;
                    var timeValue = valIdx < timeValues.Count() ? timeValues[valIdx] : string.Empty;
                    if (!string.IsNullOrEmpty(timeValue) && int.TryParse(dateValue, out intDateValue) && int.TryParse(timeValue, out intTimeValue))
                    {
                        var date = Dmi.Runtime.DmiString.PickDateToDateTime(intDateValue);
                        var time = Dmi.Runtime.DmiString.PickTimeToDateTime(intTimeValue);

                        var convertedDateTime = new DateTime(date.Year, date.Month, date.Day, time.Hours, time.Minutes, time.Seconds, DateTimeKind.Local);

                        var utcDateTimeString = convertedDateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");

                        utcDateTimeStringValues = string.Concat(utcDateTimeStringValues, _VM, utcDateTimeString);
                    }
                    else
                    {
                        // We only have a Date and not a time.
                        if (int.TryParse(dateValue, out intDateValue))
                        {
                            var date = Dmi.Runtime.DmiString.PickDateToDateTime(intDateValue);

                            var convertedDateTime = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Local);

                            var utcDateTimeString = convertedDateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");

                            utcDateTimeStringValues = string.Concat(utcDateTimeStringValues, _VM, utcDateTimeString);
                        }
                    }
                }


                var returnEthosDataRow = new EthosExtensibleDataRow(dateColumnConfig.EdmvColumnNameAssocMember,
                    dateColumnConfig.EdmvFileNameAssocMember, dateColumnConfig.EdmvJsonLabelAssocMember,
                    dateColumnConfig.EdmvJsonPathAssocMember, dateColumnConfig.EdmvJsonPropertyTypeAssocMember,
                    utcDateTimeStringValues.TrimStart(_VM))
                {
                    AssociationController = dateColumnConfig.EdmvAssociationControllerAssocMember,
                    TransType = dateColumnConfig.EdmvTransTypeAssocMember,
                    DatabaseUsageType = dateColumnConfig.EdmvDatabaseUsageTypeAssocMember,
                    Description = dateColumnConfig.EdmvColumnDescAssocMember

                };

                returnList.Add(returnEthosDataRow);
            }

            return returnList;
        }

        /// <summary>
        /// Convert the secondary key into the colleague value for comparison on association controllers
        /// </summary>
        /// <param name="columnConfigDetails"></param>
        /// <param name="colleagueValue"></param>
        /// <returns></returns>
        private async Task<string> ConvertExtendedValueToColleagueValue(EdmExtVersionsEdmvColumns columnConfigDetails, string colleagueValue)
        {
            int dateOrTimeIntFromValue;

            //switch (columnConfigDetails.EdmvJsonPropertyTypeAssocMember.ToLower())
            //{
            //    case "string":
            //        return colleagueValue;
            //    case "date":
            //        if (int.TryParse(colleagueValue, out dateOrTimeIntFromValue))
            //        {
            //            var convertedDate = Dmi.Runtime.DmiString.PickDateToDateTime(dateOrTimeIntFromValue);
            //            return convertedDate.ToString("yyyy'-'MM'-'dd");
            //        }
            //        break;
            //    case "time":
            //        if (int.TryParse(colleagueValue, out dateOrTimeIntFromValue))
            //        {
            //            var convertedDate = Dmi.Runtime.DmiString.PickTimeToDateTime(dateOrTimeIntFromValue);
            //            return convertedDate.ToString();
            //        }
            //        break;
            //    case "number":
            //        //TODO: Replace with call into DMI when correct method is made public.
            //        return ConvertColleagueNumber(colleagueValue, columnConfigDetails.EdmvConversionAssocMember);
            //    default:
            //        return colleagueValue;
            //}

            if (!string.IsNullOrEmpty(colleagueValue))
            {
                if (columnConfigDetails.EdmvJsonPropertyTypeAssocMember == "date")
                {
                    try
                    {
                        if (int.TryParse(colleagueValue, out dateOrTimeIntFromValue))
                        {
                            var convertedDate = Dmi.Runtime.DmiString.PickDateToDateTime(dateOrTimeIntFromValue);
                            return convertedDate.ToString("yyyy'-'MM'-'dd");
                        }
                    }
                    catch (FormatException)
                    {
                        return colleagueValue;
                    }
                }
                if (columnConfigDetails.EdmvJsonPropertyTypeAssocMember == "datetime")
                {
                    try
                    {
                        if (int.TryParse(colleagueValue, out dateOrTimeIntFromValue))
                        {
                            var convertedDate = Dmi.Runtime.DmiString.PickDateToDateTime(dateOrTimeIntFromValue);
                            return convertedDate.ToString("yyyy'-'MM'-'dd");
                        }
                    }
                    catch (FormatException)
                    {
                        return colleagueValue;
                    }
                }
                if (columnConfigDetails.EdmvJsonPropertyTypeAssocMember == "time")
                {
                    try
                    {
                        if (int.TryParse(colleagueValue, out dateOrTimeIntFromValue))
                        {
                            var convertedDate = Dmi.Runtime.DmiString.PickTimeToDateTime(dateOrTimeIntFromValue);
                            return convertedDate.ToString("yyyy'-'MM'-'dd");
                        }
                    }
                    catch (FormatException)
                    {
                        return colleagueValue;
                    }
                }
                if (columnConfigDetails.EdmvJsonPropertyTypeAssocMember == "number")
                {
                    try
                    {
                         return ConvertColleagueNumber(colleagueValue, columnConfigDetails.EdmvConversionAssocMember);
                    }
                    catch (FormatException)
                    {
                        return colleagueValue;
                    }
                }
                if (!string.IsNullOrEmpty(columnConfigDetails.EdmvGuidColumnNameAssocMember) && !string.IsNullOrEmpty(columnConfigDetails.EdmvGuidFileNameAssocMember))
                {
                    try
                    {
                        var recordInfo = await GetRecordInfoFromGuidAsync(colleagueValue);
                        if (recordInfo != null)
                        {
                            if (string.IsNullOrEmpty(recordInfo.SecondaryKey))
                            {
                                return recordInfo.PrimaryKey;
                            }
                            else
                            {
                                return recordInfo.SecondaryKey;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        return colleagueValue;
                    }
                }
                if (columnConfigDetails.EdmvTransEnumTableAssocMember != null && columnConfigDetails.EdmvTransEnumTableAssocMember.Any())
                {
                    try
                    {
                        var enumerations = columnConfigDetails.EdmvTransEnumTableAssocMember.Split(_SM)[0].Split(_TM).ToList();
                        var enumerationValues = columnConfigDetails.EdmvTransEnumTableAssocMember.Split(_SM)[1].Split(_TM).ToList();
                        var matchingIndex = enumerationValues.IndexOf(colleagueValue);
                        if (matchingIndex >= 0)
                        {
                            return enumerations.ElementAtOrDefault(matchingIndex);
                        }
                    }
                    catch (Exception)
                    {
                        return colleagueValue;
                    }
                }
                if (!string.IsNullOrEmpty(columnConfigDetails.EdmvTransColumnNameAssocMember))
                {
                    try
                    {
                        return await GetRecordIdFromTranslationAsync(colleagueValue, columnConfigDetails.EdmvTransFileNameAssocMember, columnConfigDetails.EdmvTransColumnNameAssocMember, columnConfigDetails.EdmvTransTableNameAssocMember);
                    }
                    catch (Exception)
                    {
                        return colleagueValue;
                    }
                }
            }

            return colleagueValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceData"></param>
        /// <param name="entityName"></param>
        /// <param name="sourceColumn"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public async Task<string> GetRecordIdFromTranslationAsync(string sourceData, string entityName, string sourceColumn = "", string tableName = "")
        {
            string recordKey = sourceData;
            if (!string.IsNullOrEmpty(tableName))
            {
                var valcodeTable = await DataReader.ReadRecordAsync<ApplValcodes>(entityName, tableName);
                switch (sourceColumn)
                {
                    case ("VAL.INTERNAL.CODE"):
                        {
                            var assoc = valcodeTable.ValsEntityAssociation.FirstOrDefault(es => es.ValInternalCodeAssocMember == sourceData);
                            if (assoc != null)
                                recordKey = assoc.ValInternalCodeAssocMember;
                            break;
                        }
                    case ("VAL.EXTERNAL.REPRESENTATION"):
                        {
                            var assoc = valcodeTable.ValsEntityAssociation.FirstOrDefault(ex => ex.ValExternalRepresentationAssocMember == sourceData);
                            if (assoc != null)
                                recordKey = assoc.ValInternalCodeAssocMember;
                            break;
                        }
                    case ("VAL.ACTION.CODE.1"):
                        {
                            var assoc = valcodeTable.ValsEntityAssociation.FirstOrDefault(ex => ex.ValActionCode1AssocMember == sourceData);
                            if (assoc != null)
                                recordKey = assoc.ValInternalCodeAssocMember;
                            break;
                        }
                    case ("VAL.ACTION.CODE.2"):
                        {
                            var assoc = valcodeTable.ValsEntityAssociation.FirstOrDefault(ex => ex.ValActionCode2AssocMember == sourceData);
                            if (assoc != null)
                                recordKey = assoc.ValInternalCodeAssocMember;
                            break;
                        }
                    case ("VAL.ACTION.CODE.3"):
                        {
                            var assoc = valcodeTable.ValsEntityAssociation.FirstOrDefault(ex => ex.ValActionCode3AssocMember == sourceData);
                            if (assoc != null)
                                recordKey = assoc.ValInternalCodeAssocMember;
                            break;
                        }
                    case ("VAL.ACTION.CODE.4"):
                        {
                            var assoc = valcodeTable.ValsEntityAssociation.FirstOrDefault(ex => ex.ValActionCode4AssocMember == sourceData);
                            if (assoc != null)
                                recordKey = assoc.ValInternalCodeAssocMember;
                            break;
                        }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(entityName))
                {
                    var recordKeys = await DataReader.SelectAsync(entityName, string.Format("WITH {0} EQ '{1}'", sourceColumn, sourceData));
                    if (recordKeys == null || !recordKeys.Any())
                    {
                        // Try translation on the record key.
                        recordKeys = await DataReader.SelectAsync(entityName, string.Format("WITH @ID EQ '{0}' SAVING {1}", sourceData, sourceColumn));
                    }
                    if (recordKeys != null && recordKeys.Any())
                    {
                        recordKey = recordKeys[0];
                    }
                }
            }
            return recordKey;
        }

        /// <summary>
        /// Encode a primary key for use in Extensibility
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Encoded string to use as guid on a non-guid based API.</returns>
        public string EncodePrimaryKey(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return id;
            }
            // Preserve all lower case and dashes in original key by manually escaping those characters.
            var returnData = id.Replace("-", "-2D").Replace("a", "-61").
                Replace("b", "-62").Replace("c", "-63").Replace("d", "-64").
                Replace("e", "-65").Replace("f", "-66").Replace("g", "-67").
                Replace("h", "-68").Replace("i", "-69").Replace("j", "-6A").
                Replace("k", "-6B").Replace("l", "-6C").Replace("m", "-6D").
                Replace("n", "-6E").Replace("o", "-6F").Replace("p", "-70").
                Replace("q", "-71").Replace("r", "-72").Replace("s", "-73").
                Replace("t", "-74").Replace("u", "-75").Replace("v", "-76").
                Replace("w", "-77").Replace("x", "-78").Replace("y", "-79").
                Replace("z", "-7A");
            return Uri.EscapeDataString(returnData).Replace("%", "-").ToLower();
        }

        /// <summary>
        /// Un-Encode a primary key for use in Extensibility
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Un-Encoded string taken from a non-guid based API guid.</returns>
        public string UnEncodePrimaryKey(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return id;
            }
            var primaryKey = id.Replace("-", "%").ToUpper();
            return Uri.UnescapeDataString(primaryKey);
        }

        private static string ConvertColleagueNumber(string value, string fmt)
        {
            //copied code from DMI ReflectableString, needs to be removed later to be replaced by correct call once method is public
            try
            {
                int precision = 0;
                // Envision style: MD is followed by the number of display decimals and an optional internal precision (MD2, MD25)
                if (fmt.Length > 2)
                {
                    string mdSize = "";
                    int digit = 0;
                    fmt = fmt.Substring(2);
                    if (fmt.Length > 0) mdSize = fmt.Substring(0, 1);
                    if (!string.IsNullOrEmpty(mdSize) && int.TryParse(mdSize, out digit))
                    {
                        precision = digit;
                        if (fmt.Length > 1) mdSize = fmt.Substring(1, 1);
                        if (!string.IsNullOrEmpty(mdSize) && int.TryParse(mdSize, out digit))
                        {
                            precision = digit;
                        }
                    }
                }

                if (precision < 1)
                    return value;

                string prefix = "";
                string leftSide = "";
                string rightSide = "";
                if (!string.IsNullOrEmpty(value) && (value[0] == '+' || value[0] == '-'))
                {
                    prefix = value.Substring(0, 1);
                    value = value.Substring(1);
                }
                if (value.IndexOf(".") > -1)
                {
                    int pos = value.IndexOf(".");
                    leftSide = value.Substring(0, pos);
                    rightSide = value.Substring(pos + 1);
                    pos = leftSide.Length - precision;
                    if (pos >= 0)
                    {
                        string migratingDigits = leftSide.Substring(pos);
                        leftSide = leftSide.Substring(0, pos);
                        rightSide = migratingDigits + rightSide;
                    }
                    else
                    {
                        pos = 0 - pos;
                        StringBuilder migratingDigits = new StringBuilder(leftSide);
                        migratingDigits.Insert(0, "0", pos);
                        leftSide = "0";
                        rightSide = migratingDigits.ToString() + rightSide;
                    }
                }
                else
                {
                    int pos = value.Length - precision;
                    if (pos >= 0)
                    {
                        leftSide = value.Substring(0, pos);
                        rightSide = value.Substring(pos);
                    }
                    else
                    {
                        pos = 0 - pos;
                        StringBuilder zeros = new StringBuilder(pos);
                        zeros.Insert(0, "0", pos);
                        leftSide = "0";
                        rightSide = zeros.ToString() + value;
                    }
                }
                return prefix + leftSide + "." + rightSide;

            }
            catch (Exception)
            {
                return value;
            }
        }

        /// <summary>
        /// Get an external mapping
        /// </summary>
        /// <param name="id">External Mapping ID</param>
        /// <returns>The external mapping info</returns>
        public ExternalMapping GetExternalMapping(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "External Mapping ID must be specified.");
            }

            ElfTranslateTables elfTranslateTable = DataReader.ReadRecord<ElfTranslateTables>(id);
            if (elfTranslateTable == null)
            {
                throw new KeyNotFoundException("External Mapping ID " + id + " is not valid.");
            }

            return BuildExternalMapping(elfTranslateTable);
        }

        /// <summary>
        /// Get the defaults configuration.
        /// </summary>
        /// <returns>The <see cref="DefaultsConfiguration">defaults configuration</see></returns>
        public DefaultsConfiguration GetDefaultsConfiguration()
        {
            return GetOrAddToCache<DefaultsConfiguration>("DefaultsConfiguration",
                () => { return BuildDefaultsConfiguration(); });
        }

        /// <summary>
        /// Gets an integration configuration
        /// </summary>
        /// <param name="integrationConfigurationId">Integration Configuration ID</param>
        /// <returns>An <see cref="IntegrationConfiguration">integration configuration</see></returns>
        public async Task<IntegrationConfiguration> GetIntegrationConfiguration(string integrationConfigurationId)
        {
            if (string.IsNullOrEmpty(integrationConfigurationId))
            {
                throw new ArgumentNullException("Integration configuration key is required.");
            }
            return await BuildIntegrationConfiguration(integrationConfigurationId);
        }

        /// <summary>
        /// Updates a single item in the list of Audit Log configuration records from Colleague
        /// </summary>
        /// <param name="auditLogConfiguration">Audit Log Configuration to update</param>
        /// <returns>An <see cref="AuditLogConfiguration">Updated Audit Log configuration</see></returns>
        public async Task<AuditLogConfiguration> UpdateAuditLogConfigurationAsync(AuditLogConfiguration auditLogConfiguration)
        {
            UpdateAuditLogParmsRequest updateRequest = new UpdateAuditLogParmsRequest();
            var auditLogCategories = await GetAuditLogCategoriesAsync(true);
            if (auditLogCategories != null && auditLogCategories.Any())
            {
                var auditCategory = auditLogCategories.FirstOrDefault(ac => ac.Guid == auditLogConfiguration.EventId);
                if (auditCategory == null)
                {
                    auditCategory = auditLogCategories.FirstOrDefault(ac => ac.Code == auditLogConfiguration.Code);
                    if (auditCategory == null)
                    {
                        auditCategory = auditLogCategories.FirstOrDefault(ac => ac.Description == auditLogConfiguration.Description);
                    }
                }
                if (auditCategory != null)
                { 
                    updateRequest.AuditLogCategory = auditCategory.Code;
                    updateRequest.AuditLogEnabled = auditLogConfiguration.IsEnabled == true ? true : false;
                    UpdateAuditLogParmsResponse updateResponse = await transactionInvoker.ExecuteAsync<UpdateAuditLogParmsRequest, UpdateAuditLogParmsResponse>(updateRequest);

                    var auditLogEntities = await GetAuditLogConfigurationAsync(true);
                    return auditLogEntities.FirstOrDefault(ale => ale.EventId == auditLogConfiguration.EventId || ale.Code == auditLogConfiguration.Code || ale.Description == auditLogConfiguration.Description);
                }
            }
            return new AuditLogConfiguration(string.Empty, string.Empty, string.Empty, false);
        }

        /// <summary>
        /// Retrieve user profile configuration servicing old versions of the API.
        /// </summary>
        /// <returns>User profile configuration</returns>
        [Obsolete("This method services version of the API prior to 1.16.")]
        public async Task<UserProfileConfiguration> GetUserProfileConfigurationAsync()
        {
            UserProfileConfiguration configuration = new UserProfileConfiguration();

            Dflts dfltsRecord = null;
            try
            {
                dfltsRecord = GetDefaults();
            }
            catch (Exception e)
            {
                logger.Info(e, "Error retrieving DFLTS record");
                return configuration;
            }

            CorewebDefaults corewebDefaultsRecord = null;
            try
            {
                corewebDefaultsRecord = await GetCorewebDefaultsAsync();
            }
            catch (Exception e)
            {
                logger.Info(e, "Error retrieving COREWEB.DEFAULTS record");
                return configuration;
            }
            if (corewebDefaultsRecord != null && dfltsRecord != null)
            {
                configuration.UpdateAddressTypeConfiguration(string.Equals(corewebDefaultsRecord.CorewebAllAddrViewable, "Y", StringComparison.OrdinalIgnoreCase),
                    corewebDefaultsRecord.CorewebAddressViewTypes,
                    string.Equals(corewebDefaultsRecord.CorewebAddrUpdatable, "Y", StringComparison.OrdinalIgnoreCase),
                    dfltsRecord.DfltsWebAdrelType);

                configuration.UpdateEmailTypeConfiguration(string.Equals(corewebDefaultsRecord.CorewebAllEmailViewable, "Y", StringComparison.OrdinalIgnoreCase),
                    corewebDefaultsRecord.CorewebEmailViewTypes,
                    string.Equals(corewebDefaultsRecord.CorewebAllEmailUpdatable, "Y", StringComparison.OrdinalIgnoreCase),
                    corewebDefaultsRecord.CorewebEmailUpdtTypes);

                configuration.UpdatePhoneTypeConfiguration(string.Equals(corewebDefaultsRecord.CorewebAllPhoneViewable, "Y", StringComparison.OrdinalIgnoreCase),
                    corewebDefaultsRecord.CorewebPhoneViewTypes,
                    corewebDefaultsRecord.CorewebPhoneUpdtTypes);

                if (corewebDefaultsRecord.CorewebUserProfileText != null)
                {
                    configuration.Text = corewebDefaultsRecord.CorewebUserProfileText;
                }

                configuration.CanUpdateAddressWithoutPermission = string.Equals(corewebDefaultsRecord.CorewebAddrUpdtNoPerm, "Y", StringComparison.OrdinalIgnoreCase);
                configuration.CanUpdateEmailWithoutPermission = string.Equals(corewebDefaultsRecord.CorewebEmailUpdtNoPerm, "Y", StringComparison.OrdinalIgnoreCase);
                configuration.CanUpdatePhoneWithoutPermission = string.Equals(corewebDefaultsRecord.CorewebPhoneUpdtNoPerm, "Y", StringComparison.OrdinalIgnoreCase);
            }
            return configuration;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        private UserProfileViewUpdateOption ConvertCodeToUserProfileViewUpdateOption(string code)
        {
            if (code == null)
            {
                return UserProfileViewUpdateOption.NotAllowed;
            }

            switch (code.ToUpperInvariant())
            {
                case "U":
                    return UserProfileViewUpdateOption.Updatable;
                case "V":
                    return UserProfileViewUpdateOption.Viewable;
                default:
                    return UserProfileViewUpdateOption.NotAllowed;
            }
        }

        /// <summary>
        /// Retrieve user profile configuration.
        /// </summary>
        /// <param name="allAdrelTypes">Address Relation Type codes</param>
        /// <returns>User profile configuration</returns>
        public async Task<UserProfileConfiguration2> GetUserProfileConfiguration2Async(List<AddressRelationType> allAdrelTypes)
        {
            UserProfileConfiguration2 configuration = new UserProfileConfiguration2();

            CorewebDefaults corewebDefaultsRecord = null;
            try
            {
                corewebDefaultsRecord = await GetCorewebDefaultsAsync();
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
            catch (Exception e)
            {
                logger.Error(e, "Error retrieving COREWEB.DEFAULTS record");
                return configuration;
            }
            if (corewebDefaultsRecord != null)
            {
                configuration.UpdateAddressTypeConfiguration(string.Equals(corewebDefaultsRecord.CorewebAllAddrViewable, "Y", StringComparison.OrdinalIgnoreCase),
                    corewebDefaultsRecord.CorewebAddressViewTypes,
                    corewebDefaultsRecord.CorewebAddressUpdtTypes,
                    allAdrelTypes);

                configuration.UpdateEmailTypeConfiguration(string.Equals(corewebDefaultsRecord.CorewebAllEmailViewable, "Y", StringComparison.OrdinalIgnoreCase),
                    corewebDefaultsRecord.CorewebEmailViewTypes,
                    string.Equals(corewebDefaultsRecord.CorewebAllEmailUpdatable, "Y", StringComparison.OrdinalIgnoreCase),
                    corewebDefaultsRecord.CorewebEmailUpdtTypes);

                configuration.UpdatePhoneTypeConfiguration(string.Equals(corewebDefaultsRecord.CorewebAllPhoneViewable, "Y", StringComparison.OrdinalIgnoreCase),
                    corewebDefaultsRecord.CorewebPhoneViewTypes,
                    corewebDefaultsRecord.CorewebPhoneUpdtTypes,
                    string.Equals(corewebDefaultsRecord.CorewebNullPhnTypeViewbl, "Y", StringComparison.OrdinalIgnoreCase));

                if (corewebDefaultsRecord.CorewebUserProfileText != null)
                {
                    configuration.Text = corewebDefaultsRecord.CorewebUserProfileText;
                    configuration.AuthorizePhonesForText = string.Equals(corewebDefaultsRecord.CorewebPhoneTextAuth, "Y", StringComparison.OrdinalIgnoreCase);
                }

                configuration.CanUpdateAddressWithoutPermission = string.Equals(corewebDefaultsRecord.CorewebAddrUpdtNoPerm, "Y", StringComparison.OrdinalIgnoreCase);
                configuration.CanUpdateEmailWithoutPermission = string.Equals(corewebDefaultsRecord.CorewebEmailUpdtNoPerm, "Y", StringComparison.OrdinalIgnoreCase);
                configuration.CanUpdatePhoneWithoutPermission = string.Equals(corewebDefaultsRecord.CorewebPhoneUpdtNoPerm, "Y", StringComparison.OrdinalIgnoreCase);
                configuration.CanViewUpdateChosenName = ConvertCodeToUserProfileViewUpdateOption(corewebDefaultsRecord.CorewebChoNameOption);
                configuration.CanViewUpdateNickname = ConvertCodeToUserProfileViewUpdateOption(corewebDefaultsRecord.CorewebNicknameOption);
                configuration.CanViewUpdatePronoun = ConvertCodeToUserProfileViewUpdateOption(corewebDefaultsRecord.CorewebPronounOption);
                configuration.CanViewUpdateGenderIdentity = ConvertCodeToUserProfileViewUpdateOption(corewebDefaultsRecord.CorewebGenIdentOption);


            }
            return configuration;
        }

        /// <summary>
        /// Gets the Emergency Information Configuration from Colleague.
        /// </summary>
        /// <returns>EmergencyInformationConfiguration</returns>
        public async Task<EmergencyInformationConfiguration> GetEmergencyInformationConfigurationAsync()
        {
            try
            {
                var corewebDefaults = await GetCorewebDefaultsAsync();
                var hideHealthConditions = string.Equals(corewebDefaults.CorewebEmerHideHlthCond, "Y", StringComparison.OrdinalIgnoreCase);
                var hideOtherInformation = string.Equals(corewebDefaults.CorewebEmerHideOtherInfo, "Y", StringComparison.OrdinalIgnoreCase);
                var requireContact = string.Equals(corewebDefaults.CorewebEmerRequireContact, "Y", StringComparison.OrdinalIgnoreCase);
                var allowOptOut = string.Equals(corewebDefaults.CorewebEmerAllowOptout, "Y", StringComparison.OrdinalIgnoreCase);
                EmergencyInformationConfiguration configuration = new EmergencyInformationConfiguration(hideHealthConditions, hideOtherInformation, requireContact, allowOptOut);
                return configuration;
            }
            catch (Exception e)
            {
                logger.Info(e, "Error retrieving COREWEB.DEFAULTS record for EmergencyInformationConfiguration");
                throw;
            }
        }

        /// <summary>
        /// Return a RestrictionConfiguration entity that contains a list of entities (SeverityStyleMapping).
        /// </summary>
        /// <returns>awaitable Restriction Configuration</returns>
        public async Task<RestrictionConfiguration> GetRestrictionConfigurationAsync()
        {
            RestrictionConfiguration configuration = new RestrictionConfiguration();

            CorewebDefaults corewebDefaultsRecord = null;
            try
            {
                corewebDefaultsRecord = await GetCorewebDefaultsAsync();
            }
            catch (Exception e)
            {
                logger.Info(e, "Error retrieving COREWEB.DEFAULTS record");
                return configuration;
            }

            if ((corewebDefaultsRecord.CorewebSeverityStart != null) &&
                (corewebDefaultsRecord.CorewebSeverityEnd != null) &&
                (corewebDefaultsRecord.CorewebStyle != null))
            {
                for (int i = 0; i < corewebDefaultsRecord.CorewebSeverityStart.Count; i++)
                {
                    AlertStyle style;

                    switch (corewebDefaultsRecord.CorewebStyle[i])
                    {
                        case "C":
                            style = AlertStyle.Critical;
                            break;
                        case "W":
                            style = AlertStyle.Warning;
                            break;
                        case "I":
                            style = AlertStyle.Information;
                            break;
                        default:
                            throw new InvalidCastException("Unknown");
                    }

                    var start = corewebDefaultsRecord.CorewebSeverityStart[i];
                    var end = corewebDefaultsRecord.CorewebSeverityEnd[i];
                    configuration.AddItem(new SeverityStyleMapping(start, end, style));
                }
            }
            return configuration;
        }

        /// <summary>
        /// Primary and secondary sort code converted to WebSortField
        /// </summary>
        /// <param name="code">Sort field code</param>
        /// <param name="type">Primary or secondary type</param>
        /// <returns></returns>
        private WebSortField ConvertCodeToWebSortField(string code, WebSortField defaultSortField)
        {
            if (code == null)
            {
                return defaultSortField;
            }

            switch (code.ToUpperInvariant())
            {
                case "DESC":
                    return WebSortField.Description;
                case "STATUS":
                    return WebSortField.Status;
                case "STATDATE":
                    return WebSortField.StatusDate;
                case "DUEDATE":
                    return WebSortField.DueDate;
                case "OFFICE":
                    return WebSortField.OfficeDescription;
                default:
                    return defaultSortField;
            }
        }

        /// <summary>
        /// Retrieve required document configuration.
        /// </summary>
        /// <returns>Required document configuration</returns>
        public async Task<RequiredDocumentConfiguration> GetRequiredDocumentConfigurationAsync()
        {
            RequiredDocumentConfiguration configuration = new RequiredDocumentConfiguration();
            // Get OFFICE.COLLECTION.MAP
            OfficeCollectionMap officeCollectionMap = null;
            try
            {
                officeCollectionMap = await GetOfficeCollectionMapAsync();
            }
            catch (Exception ex)
            {
                logger.Info(ex, "Error retrieving OFFICE.COLLECTION.MAP from CORE.PARMS.");
            }
            // Get COREWEB.DEFAULTS
            CorewebDefaults corewebDefaultsRecord = null;
            try
            {
                corewebDefaultsRecord = await GetCorewebDefaultsAsync();
            }
            catch (Exception e)
            {
                logger.Info(e, "Error retrieving COREWEB.DEFAULTS record");
                // To be consistent with earlier versions of this endpoint - when the mapping was not included - 
                // return a null object when it cannot read COREWEB.DEFAULTS.
                return null;
            }

            if (corewebDefaultsRecord != null)
            {
                configuration.SuppressInstance = string.Equals(corewebDefaultsRecord.CorewebSuppressInstance, "Y", StringComparison.OrdinalIgnoreCase);
                configuration.PrimarySortField = ConvertCodeToWebSortField(corewebDefaultsRecord.CorewebDocumentsSort1, WebSortField.Status);
                configuration.SecondarySortField = ConvertCodeToWebSortField(corewebDefaultsRecord.CorewebDocumentsSort2, WebSortField.OfficeDescription);
                configuration.TextForBlankStatus = corewebDefaultsRecord.CorewebBlankStatusText;
                configuration.TextForBlankDueDate = corewebDefaultsRecord.CorewebBlankDueDateText;
            }
            if (officeCollectionMap != null)
            {
                RequiredDocumentCollectionMapping mapping = new RequiredDocumentCollectionMapping();
                mapping.RequestsWithoutOfficeCodeCollection = officeCollectionMap.OfcoDefaultCollection;
                mapping.UnmappedOfficeCodeCollection = officeCollectionMap.OfcoDfltOfficeCollection;
                if (officeCollectionMap.OfcomapEntityAssociation != null && officeCollectionMap.OfcomapEntityAssociation.Any())
                {
                    foreach (var oa in officeCollectionMap.OfcomapEntityAssociation)
                    {
                        try
                        {
                            var officeCodeAttachmentCollection = new OfficeCodeAttachmentCollection(oa.OfcoOfficeCodesAssocMember, oa.OfcoCollectionIdsAssocMember);
                            mapping.AddOfficeCodeAttachment(officeCodeAttachmentCollection);
                        }
                        catch (Exception ae)
                        {
                            logger.Info(ae, "Not able to add to office code collection mapping - either duplicate or missing info: OfficeCode = " + oa.OfcoOfficeCodesAssocMember + " Collection = " + oa.OfcoCollectionIdsAssocMember);
                        }
                    }
                }
                configuration.RequiredDocumentCollectionMapping = mapping;
            }

            return configuration;
        }

        /// <summary>
        /// Get Pilot configuration
        /// </summary>
        /// <returns>Pilot configuration</returns>
        public async Task<PilotConfiguration> GetPilotConfigurationAsync()
        {
            PilotParms pilotParms = await GetPilotParmsAsync();
            PilotConfiguration pilotConfiguration = new PilotConfiguration();
            pilotConfiguration.PrimaryPhoneTypes = pilotParms.PilHomePhoneTypes;
            pilotConfiguration.SmsPhoneTypes = pilotParms.PilCellPhoneTypes;
            return pilotConfiguration;
        }

        /// <summary>
        /// Retrieves privacy configuration
        /// </summary>
        /// <returns>Privacy configuration</returns>
        public async Task<PrivacyConfiguration> GetPrivacyConfigurationAsync()
        {
            var defaults = await GetDefaultsAsync();
            return new PrivacyConfiguration(defaults.DfltsRecordDenialMsg);
        }

        /// <summary>
        /// Retrieves organizational relationship configuration
        /// </summary>
        /// <returns>Organizational relationship configuration</returns>
        public async Task<OrganizationalRelationshipConfiguration> GetOrganizationalRelationshipConfigurationAsync()
        {
            var relationshipCategories = await GetRelationshipCategoriesAsync();
            var organizationRelationshipConfiguration = new OrganizationalRelationshipConfiguration();
            // Populate type mapping with all special types
            foreach (OrganizationalRelationshipType orgRelType in Enum.GetValues(typeof(OrganizationalRelationshipType)))
            {
                organizationRelationshipConfiguration.RelationshipTypeCodeMapping.Add(orgRelType, new List<string>());
            }

            // Map type codes to special types
            foreach (var relationshipCategory in relationshipCategories.ValsEntityAssociation)
            {
                if (relationshipCategory.ValActionCode1AssocMember == orgIndicator)
                {
                    organizationRelationshipConfiguration.RelationshipTypeCodeMapping[OrganizationalRelationshipType.Manager].Add(relationshipCategory.ValInternalCodeAssocMember);
                }

            }
            return organizationRelationshipConfiguration;
        }

        /// <summary>
        /// Retrieves <see cref="SelfServiceConfiguration"/>
        /// </summary>
        /// <returns>A <see cref="SelfServiceConfiguration"/> object</returns>
        public async Task<SelfServiceConfiguration> GetSelfServiceConfigurationAsync()
        {
            try
            {
                Dflts defaults = await GetDefaultsAsync();
                bool alwaysUseClipboardForBulkMailToLinks = defaults != null && defaults.DfltsEmailAllAlwaysCopy != null && defaults.DfltsEmailAllAlwaysCopy.Equals("Y", StringComparison.InvariantCultureIgnoreCase);
                return new SelfServiceConfiguration(alwaysUseClipboardForBulkMailToLinks);
            }
            catch (Exception ex)
            {
                ApplicationException appEx = new ApplicationException("Error retrieving DFLTS data needed to build Self-Service Configuration object.", ex);
                logger.Error(appEx, appEx.Message);
                throw appEx;
            }
        }


        #region backup/restore configs


        /// <summary>
        /// Adds a new backup config record to the BACKUP.CONFIGURATION.DATA entity.
        /// The configuration data will be encrypted by the CTX before it is written to the DB.
        /// </summary>
        /// <param name="backupConfigToAdd"></param>
        /// <returns></returns>
        public async Task<BackupConfiguration> AddBackupConfigurationAsync(BackupConfiguration backupConfigToAdd)
        {
            if (backupConfigToAdd == null)
            {
                throw new ArgumentNullException("backupConfigToAdd");
            }
            var writeRequest = new WriteBackupConfigDataRequest()
            {
                ProductId = backupConfigToAdd.ProductId,
                ProductVersion = backupConfigToAdd.ProductVersion,
                Namespace = backupConfigToAdd.Namespace,
                ConfigVersion = backupConfigToAdd.ConfigVersion,
                ConfigData = backupConfigToAdd.ConfigData

            };
            var response = await transactionInvoker.ExecuteAsync<WriteBackupConfigDataRequest, WriteBackupConfigDataResponse>(writeRequest);

            if (!string.IsNullOrEmpty(response.Error))
            {
                var errorText = "Error writing backup data: " + response.Error;
                logger.Error(errorText);
                throw new InvalidOperationException(errorText);
            }
            var createdRecordList = await GetBackupConfigurationByIdsAsync(new List<string>() { response.ConfigDataId });
            return createdRecordList.FirstOrDefault();
        }

        /// <summary>
        /// Retrieve backup config records from BACKUP.CONFIGURATION.DATA with the specified IDs
        /// </summary>
        /// <param name="configDataIds"></param>
        /// <returns></returns>
        public async Task<List<BackupConfiguration>> GetBackupConfigurationByIdsAsync(List<string> configDataIds)
        {
            if (configDataIds == null || configDataIds.Count() == 0)
            {
                throw new ArgumentException("configDataIds cannot be null or empty.");
            }
            var readRequest = new ReadBackupConfigDataRequest()
            {
                InputConfigDataId = configDataIds,
                Namespace = null
            };
            var response = await transactionInvoker.ExecuteAsync<ReadBackupConfigDataRequest, ReadBackupConfigDataResponse>(readRequest);

            if (!string.IsNullOrEmpty(response.Error))
            {
                var errorText = "Error reading backup data: " + response.Error;
                logger.Error(errorText);
                throw new InvalidOperationException(errorText);
            }
            return ParseBackupConfigRecordsFromCtxResponse(response);
        }

        /// <summary>
        /// Retrieve a backup config record from BACKUP.CONFIGURATION.DATA with the specified namespace.
        /// </summary>
        /// <param name="nameSpace"></param>
        /// <returns></returns>
        public async Task<List<BackupConfiguration>> GetBackupConfigurationByNamespaceAsync(string nameSpace)
        {
            if (string.IsNullOrEmpty(nameSpace))
            {
                throw new ArgumentNullException("nameSpace");
            }
            var readRequest = new ReadBackupConfigDataRequest()
            {
                InputConfigDataId = null,
                Namespace = nameSpace
            };
            var response = await transactionInvoker.ExecuteAsync<ReadBackupConfigDataRequest, ReadBackupConfigDataResponse>(readRequest);

            if (!string.IsNullOrEmpty(response.Error))
            {
                var errorText = "Error reading backup data: " + response.Error;
                logger.Error(errorText);
                throw new InvalidOperationException(errorText);
            }
            else
            {
                if (response.BackupConfigRecords == null || response.BackupConfigRecords.Count == 0)
                {
                    var errorText = "No backup config records returned.";
                    logger.Error(errorText);
                    throw new InvalidOperationException(errorText);
                }

            }
            return ParseBackupConfigRecordsFromCtxResponse(response);
        }

        /// <summary>
        /// Converts the CTX reponse's assoc arrays of record fields into a list of BackupConfiguration domain objects.
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        private List<BackupConfiguration> ParseBackupConfigRecordsFromCtxResponse(ReadBackupConfigDataResponse response)
        {
            var resultList = new List<BackupConfiguration>();
            for (int i = 0; i < response.BackupConfigRecords.Count; i++)
            {
                var record = new BackupConfiguration()
                {
                    Id = response.BackupConfigRecords[i].ConfigDataId,
                    ProductId = response.BackupConfigRecords[i].ProductId,
                    ProductVersion = response.BackupConfigRecords[i].ProductVersion,
                    ConfigVersion = response.BackupConfigRecords[i].ConfigVersion,
                    Namespace = response.BackupConfigRecords[i].ConfigNamespace,
                    ConfigData = response.BackupConfigRecords[i].ConfigData,
                    CreatedDateTime = response.BackupConfigRecords[i].AddedTime.ToPointInTimeDateTimeOffset(
                        response.BackupConfigRecords[i].AddedDate, colleagueTimeZone)
                };
                resultList.Add(record);
            }
            return resultList;
        }


        #endregion

        /// <summary>
        /// Gets the Session Configuration.
        /// </summary>
        /// <returns>Session Configuration entity</returns>
        public async Task<SessionConfiguration> GetSessionConfigurationAsync()
        {
            try
            {
                var sessionConfigurationResponse = await anonymousTransactionInvoker.ExecuteAnonymousAsync<GetSessionConfigurationRequest, GetSessionConfigurationResponse>(new GetSessionConfigurationRequest());
                if (!string.IsNullOrEmpty(sessionConfigurationResponse.ErrorOccurred) && !string.Equals(sessionConfigurationResponse.ErrorOccurred, "0", StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new ApplicationException(string.Format("Error occurred during session configuration transaction: {0} {1}", sessionConfigurationResponse.ErrorOccurred, sessionConfigurationResponse.ErrorMessage));
                }
                var sessionConfiguration = new SessionConfiguration()
                {
                    UsernameRecoveryEnabled = string.Equals(sessionConfigurationResponse.UsernameRecoveryEnabled, "Y", StringComparison.InvariantCultureIgnoreCase),
                    PasswordResetEnabled = string.Equals(sessionConfigurationResponse.PasswordResetEnabled, "Y", StringComparison.InvariantCultureIgnoreCase)
                };
                return sessionConfiguration;
            }
            catch (Exception e)
            {
                logger.Error(e, "Error retrieving session configuration.");
                throw;
            }
        }

        #endregion

        #region Private Methods

        private ExternalMapping BuildExternalMapping(ElfTranslateTables table)
        {
            if (table != null)
            {
                try
                {
                    ExternalMapping externalMapping = new ExternalMapping(table.Recordkey, table.ElftDesc)
                    {
                        OriginalCodeValidationField = table.ElftOrigCodeField,
                        NewCodeValidationField = table.ElftNewCodeField
                    };

                    if (table.ElftblEntityAssociation != null && table.ElftblEntityAssociation.Count > 0)
                    {
                        foreach (var item in table.ElftblEntityAssociation)
                        {
                            externalMapping.AddItem(new ExternalMappingItem(item.ElftOrigCodesAssocMember)
                            {
                                NewCode = item.ElftNewCodesAssocMember,
                                ActionCode1 = item.ElftActionCodes1AssocMember,
                                ActionCode2 = item.ElftActionCodes2AssocMember,
                                ActionCode3 = item.ElftActionCodes3AssocMember,
                                ActionCode4 = item.ElftActionCodes4AssocMember
                            });
                        }
                    }

                    return externalMapping;
                }
                catch (Exception ex)
                {
                    string inError = "External mapping '" + table.Recordkey + "' corrupt";
                    LogDataError("External mapping", table.Recordkey, table, ex, inError);
                    throw new ConfigurationException(inError);
                }
            }
            return null;
        }

        private DefaultsConfiguration BuildDefaultsConfiguration()
        {
            DefaultsConfiguration configuration = new DefaultsConfiguration();
            var ldmDefaults = GetLdmDefaults();
            var defaults = GetDefaults();

            if (ldmDefaults != null && defaults != null)
            {
                configuration.AddressDuplicateCriteriaId = ldmDefaults.LdmdAddrDuplCriteria;
                configuration.AddressTypeMappingId = ldmDefaults.LdmdAddrTypeMapping;
                configuration.EmailAddressTypeMappingId = ldmDefaults.LdmdEmailTypeMapping;
                configuration.PersonDuplicateCriteriaId = ldmDefaults.LdmdPersonDuplCriteria;
                configuration.SubjectDepartmentMappingId = ldmDefaults.LdmdSubjDeptMapping;

                if (ldmDefaults.LdmdCollDefaultsEntityAssociation != null && ldmDefaults.LdmdCollDefaultsEntityAssociation.Count() > 0)
                {
                    foreach (var defaultMapping in ldmDefaults.LdmdCollDefaultsEntityAssociation)
                    {
                        if (defaultMapping != null && !string.IsNullOrEmpty(defaultMapping.LdmdCollFieldNameAssocMember) && !string.IsNullOrEmpty(defaultMapping.LdmdCollDefaultValueAssocMember))
                        {
                            configuration.AddDefaultMapping(defaultMapping.LdmdCollFieldNameAssocMember, defaultMapping.LdmdCollDefaultValueAssocMember);
                        }
                        else
                        {

                            var emptyConfigurationValues = ldmDefaults.LdmdCollDefaultsEntityAssociation
                                .Where(ld => !(string.IsNullOrEmpty(ld.LdmdCollFieldNameAssocMember)) && string.IsNullOrEmpty(ld.LdmdCollDefaultValueAssocMember))
                                .Select(ld => ld.LdmdCollFieldNameAssocMember);
                            var emptyConfigurationValueMessage = emptyConfigurationValues != null ?
                                string.Format(" The following configuration fields must be mapped : {0}", string.Join(", ", emptyConfigurationValues)) : "";
                            string inError = string.Concat("Defaults configuration mapping.", emptyConfigurationValueMessage);

                            LogDataError(inError, ldmDefaults.Recordkey, ldmDefaults);
                            throw new ConfigurationException(inError);
                        }
                    }
                }
                configuration.CampusCalendarId = defaults.DfltsCampusCalendar;
                configuration.HostInstitutionCodeId = defaults.DfltsHostInstitutionCode;
            }
            return configuration;
        }

        private LdmDefaults GetLdmDefaults()
        {
            var ldmDefaults = DataReader.ReadRecord<LdmDefaults>("CORE.PARMS", "LDM.DEFAULTS");
            if (ldmDefaults == null)
            {
                throw new ConfigurationException("CDM configuration setup not complete.");
            }
            return ldmDefaults;
        }

        private Dflts GetDefaults()
        {
            var defaults = DataReader.ReadRecord<Dflts>("CORE.PARMS", "DEFAULTS");
            if (defaults == null)
            {
                throw new ConfigurationException("Default configuration setup not complete.");
            }
            return defaults;
        }

        /// <summary>
        /// Gets 
        /// </summary>
        /// <returns></returns>
        private async Task<Data.Base.DataContracts.Dflts> GetDefaultsAsync()
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

        /// <summary>
        /// Builds a list of EthosSecurity Domain entities from a collection of Colleague DataContract ethossecurity objects
        /// </summary>
        /// <param name="ethosSecurities"></param>
        /// <returns>List of EthosSecurity Domain entities</returns>
        private async Task<IEnumerable<Domain.Base.Entities.EthosSecurity>> BuildEthosSecurityList(Collection<DataContracts.EthosSecurity> ethosSecurities)
        {
            var returnList = new List<Domain.Base.Entities.EthosSecurity>();
            ethosSecurities.ForEach(e =>
            {
                var secDefList = new List<EthosSecurityDefinitions>();
                e.EthsSecDefEntityAssociation.ForEach(s =>
                {
                    bool required = s.EthsSecurityLevelAssocMember.Equals("P");
                    secDefList.Add(new EthosSecurityDefinitions(s.EthsPropertiesAssocMember, s.EthsUsersAssocMember, s.EthsRolesAssocMember, s.EthsPermissionsAssocMember, required));
                });
                returnList.Add(new Domain.Base.Entities.EthosSecurity(e.EthsApiName, secDefList));
            });

            return returnList;
        }

        /// <summary>
        /// Builds an integration configuration
        /// </summary>
        /// <param name="integrationConfigurationId">Integration configuration ID</param>
        /// <returns>An <see cref="IntegrationConfiguration">integration configuration</see></returns>
        /// 
        private async Task<IntegrationConfiguration> BuildIntegrationConfiguration(string integrationConfigurationId)
        {

            IntegrationConfiguration configuration = null;

            var cdmIntegration = await DataReader.ReadRecordAsync<CdmIntegration>("CDM.INTEGRATION", integrationConfigurationId.ToUpper());
            if (cdmIntegration == null)
            {
                throw new KeyNotFoundException(string.Format("Could not read CDM.INTEGRATION for {0}.", integrationConfigurationId.ToUpper()));
            }
            List<ResourceBusinessEventMapping> mappings = new List<ResourceBusinessEventMapping>();
            if (cdmIntegration.ApiBusEventMapEntityAssociation != null && cdmIntegration.ApiBusEventMapEntityAssociation.Count() > 0)
            {
                foreach (var map in cdmIntegration.ApiBusEventMapEntityAssociation)
                {
                    var version = map.CintApiRsrcSchemaSemVerAssocMember;
                    if (string.IsNullOrEmpty(version))
                    {
                        version = "0";
                    }
                    mappings.Add(new ResourceBusinessEventMapping(map.CintApiResourceAssocMember, version,
                        map.CintApiPathAssocMember, map.CintApiBusEventsAssocMember));
                }
            }

            System.Uri uri = new System.Uri(cdmIntegration.CintServerBaseUrl);

            int guidLifespan = cdmIntegration.CintGuidLifespan != null ? cdmIntegration.CintGuidLifespan.GetValueOrDefault() : 30;

            bool? cintServerAutorecoverFlag = false;
            cintServerAutorecoverFlag = cdmIntegration.CintServerAutorecoverFlag.ToUpper() == "Y";

            bool? cintUseIntegrationHub = false;
            cintUseIntegrationHub = cdmIntegration.CintUseIntegrationHub.ToUpper() == "Y";

            configuration = new IntegrationConfiguration(cdmIntegration.Recordkey, cdmIntegration.CintDesc, uri.Host, (uri.Scheme == "https" || uri.Scheme == "amqps"), uri.Port,
                cdmIntegration.CintServerUsername, cdmIntegration.CintServerPassword, cdmIntegration.CintBusEventExchange,
                cdmIntegration.CintBusEventQueue, cdmIntegration.CintOutboundExchange, cdmIntegration.CintInboundExchange,
                cdmIntegration.CintInboundQueue, cdmIntegration.CintApiUsername, cdmIntegration.CintApiPassword,
                cdmIntegration.CintApiErp, ConvertDebugLevelStringToDebugLevelEnumValue(cdmIntegration.CintDebugLevel), guidLifespan, mappings)
            {
                AmqpMessageServerVirtualHost = cdmIntegration.CintServerVirtHost,
                AmqpMessageServerConnectionTimeout = cdmIntegration.CintServerTimeout.GetValueOrDefault(),
                AutomaticallyRecoverAmqpMessages = cintServerAutorecoverFlag.GetValueOrDefault(),
                AmqpMessageServerHeartbeat = cdmIntegration.CintServerHeartbeat.GetValueOrDefault(),
                UseIntegrationHub = cintUseIntegrationHub ?? false,
                ApiKey = cdmIntegration.CintHubApiKey,
                TokenUrl = cdmIntegration.CintHubTokenUrl,
                PublishUrl = cdmIntegration.CintHubPublishUrl,
                SubscribeUrl = cdmIntegration.CintHubSubscribeUrl,
                ErrorUrl = cdmIntegration.CintHubErrorUrl,
                HubMediaType = cdmIntegration.CintHubMediaType
            };

            if (cdmIntegration.CintBusEventRoutingKeys != null && cdmIntegration.CintBusEventRoutingKeys.Count > 0)
            {
                configuration.BusinessEventRoutingKeys.AddRange(cdmIntegration.CintBusEventRoutingKeys);
            }

            if (cdmIntegration.CintInboundRoutingKeys != null && cdmIntegration.CintInboundRoutingKeys.Count > 0)
            {
                configuration.InboundExchangeRoutingKeys.AddRange(cdmIntegration.CintInboundRoutingKeys);
            }

            return configuration;
        }

        /// <summary>
        /// Converts a Debug Level string to a corresponding AdapterDebugLevel enumeration value
        /// </summary>
        /// <param name="debugLevel">Debug Level</param>
        /// <returns>AdapterDebugLevel enumeration value</returns>
        private AdapterDebugLevel ConvertDebugLevelStringToDebugLevelEnumValue(string debugLevel)
        {
            AdapterDebugLevel level;
            switch (debugLevel)
            {
                case "FATAL":
                    level = AdapterDebugLevel.Fatal;
                    break;
                case "ERROR":
                default:
                    level = AdapterDebugLevel.Error;
                    break;
                case "WARN":
                    level = AdapterDebugLevel.Warning;
                    break;
                case "DEBUG":
                    level = AdapterDebugLevel.Debug;
                    break;
                case "TRACE":
                    level = AdapterDebugLevel.Trace;
                    break;
                case "INFO":
                    level = AdapterDebugLevel.Information;
                    break;
            }
            return level;
        }

        /// <summary>
        /// Gets a corewebdefault data contract
        /// </summary>
        /// <returns>CorewebDefaults data contract object</returns>
        private async Task<CorewebDefaults> GetCorewebDefaultsAsync()
        {
            return await GetOrAddToCacheAsync<CorewebDefaults>("CorewebDefaults", async () =>
            {
                var corewebDefaults = await DataReader.ReadRecordAsync<CorewebDefaults>("CORE.PARMS", "COREWEB.DEFAULTS");
                if (corewebDefaults != null)
                {
                    return corewebDefaults;
                }
                else
                {
                    logger.Info("Null CorewebDefaults record returned from database");
                    return new CorewebDefaults();
                }
            });
        }

        /// <summary>
        /// Gets an OfficeCollectionMapp data contract
        /// </summary>
        /// <returns>OfficeCollectionMapps data contract object</returns>
        private async Task<OfficeCollectionMap> GetOfficeCollectionMapAsync()
        {
            return await GetOrAddToCacheAsync<OfficeCollectionMap>("OfficeCollectionMap", async () =>
            {
                var officeCollectionMap = await DataReader.ReadRecordAsync<OfficeCollectionMap>("CORE.PARMS", "OFFICE.COLLECTION.MAP");
                if (officeCollectionMap != null)
                {
                    return officeCollectionMap;
                }
                else
                {
                    logger.Info("Null OfficeCollectionMap record returned from database");
                    return new OfficeCollectionMap();
                }
            });
        }

        private async Task<PilotParms> GetPilotParmsAsync()
        {
            PilotParms pilotParms = await DataReader.ReadRecordAsync<PilotParms>("ST.PARMS", "PILOT.DEFAULTS");

            if (pilotParms == null)
            {
                // PILOT.PARMS must exist for Colleague/Pilot integration to function properly
                throw new ConfigurationException("Pilot Parameters setup not complete on PIPA form in Colleague.");
            }
            return pilotParms;
        }

        private async Task<ApplValcodes> GetRelationshipCategoriesAsync()
        {

            return await GetOrAddToCacheAsync<ApplValcodes>("RelationshipCategories",
                async () =>
                {
                    ApplValcodes relationshipTable = await DataReader.ReadRecordAsync<ApplValcodes>("UT.VALCODES", "RELATIONSHIP.CATEGORIES");
                    if (relationshipTable == null)
                    {
                        var errorMessage = "Unable to access RELATIONSHIP.CATEGORIES valcode table.";
                        logger.Info(errorMessage);
                        throw new ColleagueWebApiException(errorMessage);
                    }
                    return relationshipTable;
                }, Level1CacheTimeoutValue);
        }

        private async Task<string> ValidateValcodeTable(string appl, string table)
        {
            string fileName = "VALCODES";
            var selectKeys = await DataReader.SelectAsync(fileName, "WITH VALCODE.ID EQ " + table);
            if (selectKeys != null && selectKeys.Any())
            {
                return fileName;
            }
            return string.Concat(appl, ".VALCODES");
        }

        #endregion


        ///////////////////////////////////////////////////////////////////////////////////
        ///                                                                             ///
        ///                               CF Team                                       ///                                                                             
        ///                         TAX INFORMATION VIEWS                               ///
        ///           TAX FORMS CONFIGURATION, CONSENTs, STATEMENTs, PDFs               ///
        ///                                                                             ///
        ///////////////////////////////////////////////////////////////////////////////////

        #region CF Views

        #region Tax form consent paragraphs

        /// <summary>
        /// Gets the tax form configuration parameters for the specific tax form passed in.
        /// </summary>
        /// <param name="taxForm">The tax form (W-2, 1095-C, 1098-T, etc.)</param>
        /// <returns>Consent and withheld paragraphs for the specific tax form</returns>
        public async Task<TaxFormConfiguration2> GetTaxFormConsentConfiguration2Async(string taxForm)
        {
            if (string.IsNullOrWhiteSpace(taxForm))
                throw new ArgumentNullException("taxForm", "The tax form type must be specified.");

            var configuration = new TaxFormConfiguration2(taxForm);

            switch (taxForm)
            {
                case TaxFormTypes.FormW2:
                    configuration.ConsentParagraphs = await GetOrAddToCacheAsync<TaxFormConsentParagraph>("W2ConsentParagraphs",
                     async () =>
                     {
                         var paragraph = new TaxFormConsentParagraph();

                         // Get tax form parameters from HRWEB.DEFAULTS
                         var hrWebDefaults = await DataReader.ReadRecordAsync<HrwebDefaults>("HR.PARMS", "HRWEB.DEFAULTS");
                         if (hrWebDefaults != null)
                         {

                             // Get the tax form consent paragraphs for W-2
                             paragraph.ConsentText = hrWebDefaults.HrwebW2oConText;
                             paragraph.ConsentWithheldText = hrWebDefaults.HrwebW2oWhldText;
                         }
                         return paragraph;
                     });
                    break;

                case TaxFormTypes.Form1095C:
                    configuration.ConsentParagraphs = await GetOrAddToCacheAsync<TaxFormConsentParagraph>("1095cConsentParagraphs",
                    async () =>
                    {
                        var paragraph = new TaxFormConsentParagraph();

                        // Get tax form parameters from HRWEB.DEFAULTS
                        var hrWebDefaults = await DataReader.ReadRecordAsync<HrwebDefaults>("HR.PARMS", "HRWEB.DEFAULTS");
                        if (hrWebDefaults != null)
                        {
                            // Get the tax form consent paragraphs for 1095-C
                            paragraph.ConsentText = hrWebDefaults.Hrweb1095cConText;
                            paragraph.ConsentWithheldText = hrWebDefaults.Hrweb1095cWhldText;
                        }
                        return paragraph;
                    });
                    break;
                case TaxFormTypes.Form1098:
                    configuration = await Get1098TaxFormConsentParagraphs2Async();
                    break;
                case TaxFormTypes.FormT4:
                    configuration = await GetT4TaxFormConsentParagraphs2Async();
                    break;
                case TaxFormTypes.FormT4A:
                    configuration = await GetT4ATaxFormConsentParagraphs2Async();
                    break;
                case TaxFormTypes.FormT2202A:
                    configuration = await GetT2202ATaxFormConsentParagraphs2Async();
                    break;
                case TaxFormTypes.Form1099MI:
                    configuration = await Get1099MiTaxFormConsentParagraphs2Async();
                    break;
                case TaxFormTypes.Form1099NEC:
                    configuration = await Get1099NecTaxFormConsentParagraphsAsync();
                    break;
                default:
                    throw new ArgumentException("Invalid taxform.");
            }

            return configuration;
        }

        /// <summary>
        ///  Gets 1098-T/E Tax consents Paragraph.
        /// </summary>
        /// <returns></returns>
        private async Task<TaxFormConfiguration2> Get1098TaxFormConsentParagraphs2Async()
        {

            return await GetOrAddToCacheAsync<TaxFormConfiguration2>("1098ConsentConfig",
                async () =>
                {
                    TaxFormConfiguration2 configuration = new TaxFormConfiguration2(TaxFormTypes.Form1098);
                    var paragraph = new TaxFormConsentParagraph();

                    // Get tax form parameters from HRWEB.DEFAULTS
                    var parm1098Contract = await DataReader.ReadRecordAsync<Parm1098>("ST.PARMS", "PARM.1098");
                    if (parm1098Contract != null)
                    {
                        // Get the tax form consent paragraphs for 1095-C
                        paragraph.ConsentText = parm1098Contract.P1098ConsentText;
                        paragraph.ConsentWithheldText = parm1098Contract.P1098WhldConsentText;
                    }
                    configuration.ConsentParagraphs = paragraph;

                    // Determine if consent is required based on client configuration.
                    bool consentRequired = true;
                    if (parm1098Contract != null && !string.IsNullOrWhiteSpace(parm1098Contract.P1098ReqConsentToView))
                    {
                        consentRequired = !parm1098Contract.P1098ReqConsentToView.Equals("N", StringComparison.InvariantCultureIgnoreCase);
                    }
                    configuration.IsBypassingConsentPermitted = !consentRequired;

                    return configuration;
                });

        }

        /// <summary>
        ///  Gets 1099-MISC Tax consents Paragraph.
        /// </summary>
        /// <returns></returns>
        private async Task<TaxFormConfiguration2> Get1099MiTaxFormConsentParagraphs2Async()
        {
            TaxFormConfiguration2 configuration = new TaxFormConfiguration2(TaxFormTypes.Form1099MI);
            configuration.ConsentParagraphs = await GetOrAddToCacheAsync<TaxFormConsentParagraph>("1099MiConsentParagraphs",
                async () =>
                {
                    var paragraph = new TaxFormConsentParagraph();

                    // Get tax form parameters from HRWEB.DEFAULTS
                    var parm1099MiContract = await DataReader.ReadRecordAsync<Parm1099mi>("CF.PARMS", "PARM.1099MI");
                    if (parm1099MiContract != null)
                    {
                        // Get the tax form consent paragraphs for 1099MI
                        paragraph.ConsentText = parm1099MiContract.P1099miConsentText;
                        paragraph.ConsentWithheldText = parm1099MiContract.P1099miWhldConsentText;
                    }
                    return paragraph;
                });

            return configuration;
        }

        /// <summary>
        ///  Gets 1099Nec Tax consents Paragraph.
        /// </summary>
        /// <returns></returns>
        private async Task<TaxFormConfiguration2> Get1099NecTaxFormConsentParagraphsAsync()
        {
            TaxFormConfiguration2 configuration = new TaxFormConfiguration2(TaxFormTypes.Form1099NEC);
            configuration.ConsentParagraphs = await GetOrAddToCacheAsync<TaxFormConsentParagraph>("1099NecConsentParagraphs",
                async () =>
                {
                    var paragraph = new TaxFormConsentParagraph();

                    // Get tax form parameters from HRWEB.DEFAULTS
                    var parm1099NecContract = await DataReader.ReadRecordAsync<Parm1099nec>("CF.PARMS", "PARM.1099NEC");
                    if (parm1099NecContract != null)
                    {
                        // Get the tax form consent paragraphs for 1099MI
                        paragraph.ConsentText = parm1099NecContract.P1099necConsentText;
                        paragraph.ConsentWithheldText = parm1099NecContract.P1099necWhldConsentText;
                    }
                    return paragraph;
                });
            return configuration;
        }

        private async Task<TaxFormConfiguration2> GetT4TaxFormConsentParagraphs2Async()
        {
            TaxFormConfiguration2 configuration = await GetOrAddToCacheAsync<TaxFormConfiguration2>("T4TaxFormConfiguration",
                async () =>
                {
                    TaxFormConfiguration2 config = new TaxFormConfiguration2(TaxFormTypes.FormT4);

                    var paragraph = new TaxFormConsentParagraph();

                    // Get tax form parameters from PARM.T4
                    var contract = await DataReader.ReadRecordAsync<ParmT4>("HR.PARMS", "T4");
                    if (contract != null)
                    {
                        // Get the tax form consent paragraphs for T4
                        paragraph.ConsentText = contract.Pt4ConText;
                        paragraph.ConsentWithheldText = contract.Pt4WhldText;

                        // Get the parameter to show or hide consent information.
                        if (!(string.IsNullOrWhiteSpace(contract.Pt4HideConsentFlag)) && (contract.Pt4HideConsentFlag.ToUpperInvariant() == "Y"))
                        {
                            config.HideConsent = true;
                        }
                    }
                    config.ConsentParagraphs = paragraph;
                    return config;
                });

            return configuration;
        }

        private async Task<TaxFormConfiguration2> GetT4ATaxFormConsentParagraphs2Async()
        {
            TaxFormConfiguration2 configuration = await GetOrAddToCacheAsync<TaxFormConfiguration2>("T4ATaxFormConfiguration",
                async () =>
                {
                    TaxFormConfiguration2 config = new TaxFormConfiguration2(TaxFormTypes.FormT4A);

                    var paragraph = new TaxFormConsentParagraph();

                    // Get tax form parameters from PARM.T4A
                    var contract = await DataReader.ReadRecordAsync<ParmT4a>("CF.PARMS", "T4A");
                    if (contract != null)
                    {
                        // Get the tax form consent paragraphs for T4A
                        paragraph.ConsentText = contract.Pt4aConText;
                        paragraph.ConsentWithheldText = contract.Pt4aWhldText;

                        // Get the parameter to display or hide consent information.
                        if (!string.IsNullOrEmpty(contract.Pt4aHideConsentFlag) && contract.Pt4aHideConsentFlag.ToUpperInvariant() == "Y")
                        {
                            config.HideConsent = true;
                        }
                    }
                    config.ConsentParagraphs = paragraph;
                    return config;
                });

            return configuration;
        }

        private async Task<TaxFormConfiguration2> GetT2202ATaxFormConsentParagraphs2Async()
        {
            TaxFormConfiguration2 configuration = await GetOrAddToCacheAsync<TaxFormConfiguration2>("T2202TaxFormConfiguration",
                async () =>
                {
                    TaxFormConfiguration2 config = new TaxFormConfiguration2(TaxFormTypes.FormT2202A);

                    var paragraph = new TaxFormConsentParagraph();

                    // Get tax form parameters from CNST.RPT.PARMS
                    var contract = await DataReader.ReadRecordAsync<CnstRptParms>("ST.PARMS", "CNST.RPT.PARMS");
                    if (contract != null)
                    {
                        // Get the tax form consent paragraphs for T2202A
                        paragraph.ConsentText = contract.CnstConsentText;
                        paragraph.ConsentWithheldText = contract.CnstWhldConsentText;

                        // Get the parameter to display or hide consent information.
                        if (!string.IsNullOrEmpty(contract.CnstHideConsentFlag) && contract.CnstHideConsentFlag.ToUpperInvariant() == "Y")
                        {
                            config.HideConsent = true;
                        }
                    }
                    config.ConsentParagraphs = paragraph;
                    return config;
                });

            return configuration;
        }

        #endregion

        #region Tax form availability

        /// <summary>
        /// Gets the tax form configuration parameters for the specific tax form passed in.
        /// </summary>
        /// <param name="taxForm">The tax form (W-2, 1095-C, 1098-T, etc.)</param>
        /// <returns>Availability dates for the specific tax form</returns>
        public async Task<TaxFormConfiguration2> GetTaxFormAvailabilityConfiguration2Async(string taxForm)
        {
            if (string.IsNullOrWhiteSpace(taxForm))
                throw new ArgumentNullException("taxForm", "The tax form type must be specified.");

            var configuration = new TaxFormConfiguration2(taxForm);

            switch (taxForm)
            {
                case TaxFormTypes.FormW2:
                    // Obtain the availability dates for W-2.
                    var qtdYtdParameterW2 = await DataReader.ReadRecordAsync<QtdYtdParameterW2>("HR.PARMS", "QTD.YTD.PARAMETER");
                    if (qtdYtdParameterW2 != null)
                    {
                        // Validate the availability dates for W-2.
                        if (qtdYtdParameterW2.WebW2ParametersEntityAssociation != null)
                        {
                            foreach (var dataContract in qtdYtdParameterW2.WebW2ParametersEntityAssociation)
                            {
                                try
                                {
                                    if (string.IsNullOrEmpty(dataContract.QypWebW2YearsAssocMember))
                                        throw new ArgumentNullException("QypWebW2YearsAssocMember", "QypWebW2YearsAssocMember is required.");

                                    if (!dataContract.QypWebW2AvailableDatesAssocMember.HasValue)
                                        throw new ArgumentNullException("QypWebW2AvailableDatesAssocMember", "QypWebW2AvailableDatesAssocMember is required.");

                                    configuration.AddAvailability(new TaxFormAvailability(dataContract.QypWebW2YearsAssocMember, dataContract.QypWebW2AvailableDatesAssocMember.Value));
                                }
                                catch (Exception e)
                                {
                                    LogDataError("QypWebW2YearsAssocMember", "HR.PARMS - QTD.YTD.PARAMETER", dataContract, e, e.Message);
                                }
                            }
                        }
                    }
                    break;
                case TaxFormTypes.Form1095C:
                    // Obtain the availability dates for 1095-C.
                    var qtdYtdParameter1095C = await DataReader.ReadRecordAsync<QtdYtdParameter1095C>("HR.PARMS", "QTD.YTD.PARAMETER");
                    if (qtdYtdParameter1095C != null)
                    {
                        // Validate the availability dates for 1095-C.
                        if (qtdYtdParameter1095C.Qyp1095cParametersEntityAssociation != null)
                        {
                            foreach (var contract in qtdYtdParameter1095C.Qyp1095cParametersEntityAssociation)
                            {
                                try
                                {
                                    if (string.IsNullOrEmpty(contract.QypWeb1095cYearsAssocMember))
                                        throw new ArgumentNullException("QypWeb1095cYearsAssocMember", "QypWeb1095cYearsAssocMember is required.");

                                    if (!contract.QypWeb1095cAvailDatesAssocMember.HasValue)
                                        throw new ArgumentNullException("QypWeb1095cAvailDatesAssocMember", "QypWeb1095cAvailDatesAssocMember is required.");

                                    configuration.AddAvailability(new TaxFormAvailability(contract.QypWeb1095cYearsAssocMember, contract.QypWeb1095cAvailDatesAssocMember.Value));
                                }
                                catch (Exception e)
                                {
                                    LogDataError("Qyp1095cParametersEntityAssociation", "HR.PARMS - QTD.YTD.PARAMETER", contract, e, e.Message);
                                }
                            }
                        }
                    }
                    break;
                case TaxFormTypes.Form1098T:
                    configuration = await Get1098TaxFormAvailability2Async(TaxFormTypes.Form1098T);
                    break;
                case TaxFormTypes.Form1098E:
                    configuration = await Get1098TaxFormAvailability2Async(TaxFormTypes.Form1098E);
                    break;
                case TaxFormTypes.FormT4:
                    configuration = await GetT4TaxFormAvailability2Async();
                    break;
                case TaxFormTypes.FormT2202A:
                    // Read the CNST.RPT.PARMS record to get the list of tax years for which T2202A
                    // tax forms are online.
                    var t2202aParameter = await DataReader.ReadRecordAsync<CnstRptParms>("ST.PARMS", "CNST.RPT.PARMS");
                    if (t2202aParameter != null)
                    {
                        // Throw an exception if there a row of tax year information that does not have
                        // a tax year or an online availability flag.
                        if (t2202aParameter.CnstT2202aPdfParmsEntityAssociation != null)
                        {
                            foreach (var contract in t2202aParameter.CnstT2202aPdfParmsEntityAssociation)
                            {
                                try
                                {
                                    if (string.IsNullOrEmpty(contract.CnstT2202aPdfTaxYearAssocMember))
                                        throw new ArgumentNullException("CnstT2202aPdfTaxYearAssocMember", "CnstT2202aPdfTaxYearAssocMember is required.");

                                    if (string.IsNullOrEmpty(contract.CnstT2202aPdfWebFlagAssocMember))
                                        throw new ArgumentNullException("CnstT2202aPdfWebFlagAssocMember", "CnstT2202aPdfWebFlagAssocMember is required.");

                                    var available = false;
                                    if (contract.CnstT2202aPdfWebFlagAssocMember.ToUpper().Equals("Y"))
                                    {
                                        available = true;
                                    }
                                    configuration.AddAvailability(new TaxFormAvailability(contract.CnstT2202aPdfTaxYearAssocMember, available));
                                }
                                catch (Exception e)
                                {
                                    LogDataError("CnstT2202aPdfParmsEntityAssociation", "ST.PARMS - CNST.RPT.PARMS", contract, e, e.Message);
                                }
                            }
                        }
                    }
                    break;
            }
            return configuration;
        }

        private async Task<TaxFormConfiguration2> Get1098TaxFormAvailability2Async(string taxForm)
        {
            if (string.IsNullOrWhiteSpace(taxForm))
                throw new ArgumentNullException("taxForm", "The tax form type must be specified.");

            TaxFormConfiguration2 configuration = new TaxFormConfiguration2(taxForm);

            // Read the PARM.1098 record so we can use the tax form specified as the 1098 tax form in Colleague.
            var parm1098Contract = await DataReader.ReadRecordAsync<Parm1098>("ST.PARMS", "PARM.1098");
            if (parm1098Contract == null)
            {
                LogDataError("ST.PARMS", "PARM.1098", "", null, "PARM.1098 cannot be null.");
                return configuration;
            }
            string paramContract1098TaxForm = string.Empty, formatted1098Name = string.Empty;
            if (taxForm == TaxFormTypes.Form1098T)
            {
                paramContract1098TaxForm = parm1098Contract.P1098TTaxForm;
                formatted1098Name = "1098-T";
            }
            else if (taxForm == TaxFormTypes.Form1098E)
            {
                paramContract1098TaxForm = parm1098Contract.P1098ETaxForm;
                formatted1098Name = "1098-E";
            }

            if (string.IsNullOrEmpty(paramContract1098TaxForm))
            {
                LogDataError("ST.PARMS", "PARM.1098", "", null, "No " + formatted1098Name + "form specified.");
                return configuration;
            }

            // Obtain the availability dates for 1098.
            var taxForm1098Years = await DataReader.BulkReadRecordAsync<TaxForm1098Years>("WITH TF98Y.TAX.FORM EQ '" + paramContract1098TaxForm + "'");
            if (taxForm1098Years != null)
            {
                var taxForm1098Status = await DataReader.ReadRecordAsync<TaxFormStatus>(paramContract1098TaxForm);
                // Validate the availability dates for 1098.
                foreach (var taxFormYear in taxForm1098Years)
                {
                    try
                    {
                        if (taxFormYear.Tf98yTaxYear == null)
                        {
                            throw new NullReferenceException("Tf98yTaxYear cannot be null.");
                        }
                        if (taxFormYear.Tf98yWebEnabled == null)
                        {
                            throw new NullReferenceException("Tf98yWebEnabled cannot be null.");
                        }
                        var available = true;
                        if (taxFormYear.Tf98yWebEnabled.ToUpper().Equals("Y"))
                        {
                            // If the tax form status doesn't exist, then the tax form is not available.
                            if (taxForm1098Status == null)
                            {
                                LogDataError("TaxFormStatus", "taxForm1098Status", taxForm1098Status);
                                available = false;
                            }
                            // If the tax form status year exists and is equal to the tax form year from taxForm1098Years, then we have additional evaluation.
                            if (taxForm1098Status.TfsTaxYear != null && taxForm1098Status.TfsTaxYear == taxFormYear.Tf98yTaxYear.ToString())
                            {
                                // If there is no gen date, the tax form is not available.
                                if (taxForm1098Status.TfsGenDate == null)
                                {
                                    available = false;
                                }
                                // If the tax form status exists and is "GEN", "MOD", or "UNF" then the tax form is not available.
                                if (taxForm1098Status.TfsStatus != null &&
                                    (taxForm1098Status.TfsStatus.ToUpper() == "GEN" ||
                                    taxForm1098Status.TfsStatus.ToUpper() == "MOD" ||
                                    taxForm1098Status.TfsStatus.ToUpper() == "UNF"))
                                {
                                    available = false;
                                }
                            }
                        }
                        else
                        {
                            // If the tax form year is specified as not web enabled, the tax form is not available.
                            available = false;
                        }
                        // Create a tax form availability object and add it to the tax form configuration.
                        configuration.AddAvailability(new TaxFormAvailability(taxFormYear.Tf98yTaxYear.ToString(), available));
                    }
                    catch (NullReferenceException e)
                    {
                        LogDataError("TaxForm1098Years", "contract", taxFormYear, e, e.Message);
                    }
                }
            }
            return configuration;
        }

        private async Task<TaxFormConfiguration2> GetT4TaxFormAvailability2Async()
        {
            TaxFormConfiguration2 configuration = new TaxFormConfiguration2(TaxFormTypes.FormT4);

            var qtdYtdParameterT4 = await DataReader.ReadRecordAsync<QtdYtdParameterT4>("HR.PARMS", "QTD.YTD.PARAMETER");
            if (qtdYtdParameterT4 != null)
            {
                // Validate the availability dates for T4.
                if (qtdYtdParameterT4.WebT4ParameterEntityAssociation != null)
                {
                    foreach (var dataContract in qtdYtdParameterT4.WebT4ParameterEntityAssociation)
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(dataContract.QypWebT4YearsAssocMember))
                                throw new ArgumentNullException("QypWebT4YearsAssocMember", "QypWebT4YearsAssocMember is required.");

                            if (!dataContract.QypWebT4AvailableDatesAssocMember.HasValue)
                                throw new ArgumentNullException("QypWebT4AvailableDatesAssocMember", "QypWebT4AvailableDatesAssocMember is required.");

                            configuration.AddAvailability(new TaxFormAvailability(dataContract.QypWebT4YearsAssocMember, dataContract.QypWebT4AvailableDatesAssocMember.Value));
                        }
                        catch (Exception e)
                        {
                            LogDataError("QypWebT4YearsAssocMember", "HR.PARMS - QTD.YTD.PARAMETER", dataContract, e, e.Message);
                        }
                    }
                }
            }

            return configuration;
        }
        #endregion

        #region OBSOLETE METHODS

        /// <summary>
        /// Gets the tax form configuration parameters for the specific tax form passed in.
        /// </summary>
        /// <param name="taxFormId">The tax form (W-2, 1095-C, 1098-T, etc.)</param>
        /// <returns>Consent and withheld paragraphs for the specific tax form</returns>
        [Obsolete("Obsolete as of API 1.29.1. Use GetTaxFormConsentConfiguration2Async instead.")]
        public async Task<TaxFormConfiguration> GetTaxFormConsentConfigurationAsync(TaxForms taxFormId)
        {
            var configuration = new TaxFormConfiguration(taxFormId);

            switch (taxFormId)
            {
                case TaxForms.FormW2:
                    configuration.ConsentParagraphs = await GetOrAddToCacheAsync<TaxFormConsentParagraph>("W2ConsentParagraphs",
                     async () =>
                     {
                         var paragraph = new TaxFormConsentParagraph();

                         // Get tax form parameters from HRWEB.DEFAULTS
                         var hrWebDefaults = await DataReader.ReadRecordAsync<HrwebDefaults>("HR.PARMS", "HRWEB.DEFAULTS");
                         if (hrWebDefaults != null)
                         {

                             // Get the tax form consent paragraphs for W-2
                             paragraph.ConsentText = hrWebDefaults.HrwebW2oConText;
                             paragraph.ConsentWithheldText = hrWebDefaults.HrwebW2oWhldText;
                         }
                         return paragraph;
                     });
                    break;

                case TaxForms.Form1095C:
                    configuration.ConsentParagraphs = await GetOrAddToCacheAsync<TaxFormConsentParagraph>("1095cConsentParagraphs",
                    async () =>
                    {
                        var paragraph = new TaxFormConsentParagraph();

                        // Get tax form parameters from HRWEB.DEFAULTS
                        var hrWebDefaults = await DataReader.ReadRecordAsync<HrwebDefaults>("HR.PARMS", "HRWEB.DEFAULTS");
                        if (hrWebDefaults != null)
                        {
                            // Get the tax form consent paragraphs for 1095-C
                            paragraph.ConsentText = hrWebDefaults.Hrweb1095cConText;
                            paragraph.ConsentWithheldText = hrWebDefaults.Hrweb1095cWhldText;
                        }
                        return paragraph;
                    });
                    break;
                case TaxForms.Form1098:
                    configuration = await Get1098TaxFormConsentParagraphsAsync();
                    break;
                case TaxForms.FormT4:
                    configuration = await GetT4TaxFormConsentParagraphsAsync();
                    break;
                case TaxForms.FormT4A:
                    configuration = await GetT4ATaxFormConsentParagraphsAsync();
                    break;
                case TaxForms.FormT2202A:
                    configuration = await GetT2202ATaxFormConsentParagraphsAsync();
                    break;
                case TaxForms.Form1099MI:
                    configuration = await Get1099MiTaxFormConsentParagraphsAsync();
                    break;
            }

            return configuration;
        }

        [Obsolete("Obsolete as of API 1.29.1. Use Get1098TaxFormConsentParagraphs2Async instead.")]
        private async Task<TaxFormConfiguration> Get1098TaxFormConsentParagraphsAsync()
        {
            TaxFormConfiguration configuration = new TaxFormConfiguration(TaxForms.Form1098);
            configuration.ConsentParagraphs = await GetOrAddToCacheAsync<TaxFormConsentParagraph>("1098ConsentParagraphs",
                async () =>
                {
                    var paragraph = new TaxFormConsentParagraph();

                    // Get tax form parameters from HRWEB.DEFAULTS
                    var parm1098Contract = await DataReader.ReadRecordAsync<Parm1098>("ST.PARMS", "PARM.1098");
                    if (parm1098Contract != null)
                    {
                        // Get the tax form consent paragraphs for 1095-C
                        paragraph.ConsentText = parm1098Contract.P1098ConsentText;
                        paragraph.ConsentWithheldText = parm1098Contract.P1098WhldConsentText;
                    }
                    return paragraph;
                });

            return configuration;
        }

        /// <summary>
        ///  Gets 1099Mi Tax consents Paragraph.
        /// </summary>
        /// <returns></returns>
        [Obsolete("Obsolete as of API 1.29.1. Use Get1099MiTaxFormConsentParagraphs2Async instead.")]
        private async Task<TaxFormConfiguration> Get1099MiTaxFormConsentParagraphsAsync()
        {
            TaxFormConfiguration configuration = new TaxFormConfiguration(TaxForms.Form1099MI);
            configuration.ConsentParagraphs = await GetOrAddToCacheAsync<TaxFormConsentParagraph>("1099MiConsentParagraphs",
                async () =>
                {
                    var paragraph = new TaxFormConsentParagraph();

                    // Get tax form parameters from HRWEB.DEFAULTS
                    var parm1099MiContract = await DataReader.ReadRecordAsync<Parm1099mi>("CF.PARMS", "PARM.1099MI");
                    if (parm1099MiContract != null)
                    {
                        // Get the tax form consent paragraphs for 1099MI
                        paragraph.ConsentText = parm1099MiContract.P1099miConsentText;
                        paragraph.ConsentWithheldText = parm1099MiContract.P1099miWhldConsentText;
                    }
                    return paragraph;
                });

            return configuration;
        }

        [Obsolete("Obsolete as of API 1.29.1. Use GetT4TaxFormConsentParagraphs2Async instead.")]
        private async Task<TaxFormConfiguration> GetT4TaxFormConsentParagraphsAsync()
        {
            TaxFormConfiguration configuration = new TaxFormConfiguration(TaxForms.FormT4);
            configuration.ConsentParagraphs = await GetOrAddToCacheAsync<TaxFormConsentParagraph>("T4ConsentParagraphs",
                async () =>
                {
                    var paragraph = new TaxFormConsentParagraph();

                    // Get tax form parameters from PARM.T4
                    var contract = await DataReader.ReadRecordAsync<ParmT4>("HR.PARMS", "T4");
                    if (contract != null)
                    {
                        // Get the tax form consent paragraphs for T4
                        paragraph.ConsentText = contract.Pt4ConText;
                        paragraph.ConsentWithheldText = contract.Pt4WhldText;
                    }
                    return paragraph;
                });

            return configuration;
        }

        [Obsolete("Obsolete as of API 1.29.1. Use GetT4ATaxFormConsentParagraphs2Async instead.")]
        private async Task<TaxFormConfiguration> GetT4ATaxFormConsentParagraphsAsync()
        {
            TaxFormConfiguration configuration = new TaxFormConfiguration(TaxForms.FormT4A);
            configuration.ConsentParagraphs = await GetOrAddToCacheAsync<TaxFormConsentParagraph>("T4AConsentParagraphs",
                async () =>
                {
                    var paragraph = new TaxFormConsentParagraph();

                    // Get tax form parameters from PARM.T4A
                    var contract = await DataReader.ReadRecordAsync<ParmT4a>("CF.PARMS", "T4A");
                    if (contract != null)
                    {
                        // Get the tax form consent paragraphs for T4A
                        paragraph.ConsentText = contract.Pt4aConText;
                        paragraph.ConsentWithheldText = contract.Pt4aWhldText;
                    }
                    return paragraph;
                });

            return configuration;
        }

        [Obsolete("Obsolete as of API 1.29.1. Use GetT2202ATaxFormConsentParagraphs2Async instead.")]
        private async Task<TaxFormConfiguration> GetT2202ATaxFormConsentParagraphsAsync()
        {
            TaxFormConfiguration configuration = new TaxFormConfiguration(TaxForms.FormT2202A);
            configuration.ConsentParagraphs = await GetOrAddToCacheAsync<TaxFormConsentParagraph>("T2202AConsentParagraphs",
                async () =>
                {
                    var paragraph = new TaxFormConsentParagraph();

                    // Get tax form parameters from CNST.RPT.PARMS
                    var contract = await DataReader.ReadRecordAsync<CnstRptParms>("ST.PARMS", "CNST.RPT.PARMS");
                    if (contract != null)
                    {
                        // Get the tax form consent paragraphs for T2202A
                        paragraph.ConsentText = contract.CnstConsentText;
                        paragraph.ConsentWithheldText = contract.CnstWhldConsentText;
                    }
                    return paragraph;
                });

            return configuration;
        }

        /// <summary>
        /// Gets the tax form configuration parameters for the specific tax form passed in.
        /// </summary>
        /// <param name="taxFormId">The tax form (W-2, 1095-C, 1098-T, etc.)</param>
        /// <returns>Availability dates for the specific tax form</returns>
        [Obsolete("Obsolete as of API 1.29.1. Use GetTaxFormAvailabilityConfiguration2Async instead.")]
        public async Task<TaxFormConfiguration> GetTaxFormAvailabilityConfigurationAsync(TaxForms taxFormId)
        {
            var configuration = new TaxFormConfiguration(taxFormId);

            switch (taxFormId)
            {
                case TaxForms.FormW2:
                    // Obtain the availability dates for W-2.
                    var qtdYtdParameterW2 = await DataReader.ReadRecordAsync<QtdYtdParameterW2>("HR.PARMS", "QTD.YTD.PARAMETER");
                    if (qtdYtdParameterW2 != null)
                    {
                        // Validate the availability dates for W-2.
                        if (qtdYtdParameterW2.WebW2ParametersEntityAssociation != null)
                        {
                            foreach (var dataContract in qtdYtdParameterW2.WebW2ParametersEntityAssociation)
                            {
                                try
                                {
                                    if (string.IsNullOrEmpty(dataContract.QypWebW2YearsAssocMember))
                                        throw new ArgumentNullException("QypWebW2YearsAssocMember", "QypWebW2YearsAssocMember is required.");

                                    if (!dataContract.QypWebW2AvailableDatesAssocMember.HasValue)
                                        throw new ArgumentNullException("QypWebW2AvailableDatesAssocMember", "QypWebW2AvailableDatesAssocMember is required.");

                                    configuration.AddAvailability(new TaxFormAvailability(dataContract.QypWebW2YearsAssocMember, dataContract.QypWebW2AvailableDatesAssocMember.Value));
                                }
                                catch (Exception e)
                                {
                                    LogDataError("QypWebW2YearsAssocMember", "HR.PARMS - QTD.YTD.PARAMETER", dataContract, e, e.Message);
                                }
                            }
                        }
                    }
                    break;
                case TaxForms.Form1095C:
                    // Obtain the availability dates for 1095-C.
                    var qtdYtdParameter1095C = await DataReader.ReadRecordAsync<QtdYtdParameter1095C>("HR.PARMS", "QTD.YTD.PARAMETER");
                    if (qtdYtdParameter1095C != null)
                    {
                        // Validate the availability dates for 1095-C.
                        if (qtdYtdParameter1095C.Qyp1095cParametersEntityAssociation != null)
                        {
                            foreach (var contract in qtdYtdParameter1095C.Qyp1095cParametersEntityAssociation)
                            {
                                try
                                {
                                    if (string.IsNullOrEmpty(contract.QypWeb1095cYearsAssocMember))
                                        throw new ArgumentNullException("QypWeb1095cYearsAssocMember", "QypWeb1095cYearsAssocMember is required.");

                                    if (!contract.QypWeb1095cAvailDatesAssocMember.HasValue)
                                        throw new ArgumentNullException("QypWeb1095cAvailDatesAssocMember", "QypWeb1095cAvailDatesAssocMember is required.");

                                    configuration.AddAvailability(new TaxFormAvailability(contract.QypWeb1095cYearsAssocMember, contract.QypWeb1095cAvailDatesAssocMember.Value));
                                }
                                catch (Exception e)
                                {
                                    LogDataError("Qyp1095cParametersEntityAssociation", "HR.PARMS - QTD.YTD.PARAMETER", contract, e, e.Message);
                                }
                            }
                        }
                    }
                    break;
                case TaxForms.Form1098T:
                    configuration = await Get1098TaxFormAvailabilityAsync(TaxForms.Form1098T);
                    break;
                case TaxForms.Form1098E:
                    configuration = await Get1098TaxFormAvailabilityAsync(TaxForms.Form1098E);
                    break;
                case TaxForms.FormT4:
                    configuration = await GetT4TaxFormAvailabilityAsync();
                    break;
                case TaxForms.FormT2202A:
                    // Read the CNST.RPT.PARMS record to get the list of tax years for which T2202A
                    // tax forms are online.
                    var t2202aParameter = await DataReader.ReadRecordAsync<CnstRptParms>("ST.PARMS", "CNST.RPT.PARMS");
                    if (t2202aParameter != null)
                    {
                        // Throw an exception if there a row of tax year information that does not have
                        // a tax year or an online availability flag.
                        if (t2202aParameter.CnstT2202aPdfParmsEntityAssociation != null)
                        {
                            foreach (var contract in t2202aParameter.CnstT2202aPdfParmsEntityAssociation)
                            {
                                try
                                {
                                    if (string.IsNullOrEmpty(contract.CnstT2202aPdfTaxYearAssocMember))
                                        throw new ArgumentNullException("CnstT2202aPdfTaxYearAssocMember", "CnstT2202aPdfTaxYearAssocMember is required.");

                                    if (string.IsNullOrEmpty(contract.CnstT2202aPdfWebFlagAssocMember))
                                        throw new ArgumentNullException("CnstT2202aPdfWebFlagAssocMember", "CnstT2202aPdfWebFlagAssocMember is required.");

                                    var available = false;
                                    if (contract.CnstT2202aPdfWebFlagAssocMember.ToUpper().Equals("Y"))
                                    {
                                        available = true;
                                    }
                                    configuration.AddAvailability(new TaxFormAvailability(contract.CnstT2202aPdfTaxYearAssocMember, available));
                                }
                                catch (Exception e)
                                {
                                    LogDataError("CnstT2202aPdfParmsEntityAssociation", "ST.PARMS - CNST.RPT.PARMS", contract, e, e.Message);
                                }
                            }
                        }
                    }
                    break;
            }
            return configuration;
        }

        [Obsolete("Obsolete as of API 1.29.1. Use Get1098TaxFormAvailability2Async instead.")]
        private async Task<TaxFormConfiguration> Get1098TaxFormAvailabilityAsync(TaxForms taxFormId)
        {
            TaxFormConfiguration configuration = new TaxFormConfiguration(taxFormId);

            // Read the PARM.1098 record so we can use the tax form specified as the 1098 tax form in Colleague.
            var parm1098Contract = await DataReader.ReadRecordAsync<Parm1098>("ST.PARMS", "PARM.1098");
            if (parm1098Contract == null)
            {
                LogDataError("ST.PARMS", "PARM.1098", "", null, "PARM.1098 cannot be null.");
                return configuration;
            }
            string paramContract1098TaxForm = string.Empty, formatted1098Name = string.Empty;
            if (taxFormId == TaxForms.Form1098T)
            {
                paramContract1098TaxForm = parm1098Contract.P1098TTaxForm;
                formatted1098Name = "1098-T";
            }
            else if (taxFormId == TaxForms.Form1098E)
            {
                paramContract1098TaxForm = parm1098Contract.P1098ETaxForm;
                formatted1098Name = "1098-E";
            }

            if (string.IsNullOrEmpty(paramContract1098TaxForm))
            {
                LogDataError("ST.PARMS", "PARM.1098", "", null, "No " + formatted1098Name + "form specified.");
                return configuration;
            }

            // Obtain the availability dates for 1098.
            var taxForm1098Years = await DataReader.BulkReadRecordAsync<TaxForm1098Years>("WITH TF98Y.TAX.FORM EQ '" + paramContract1098TaxForm + "'");
            if (taxForm1098Years != null)
            {
                var taxForm1098Status = await DataReader.ReadRecordAsync<TaxFormStatus>(paramContract1098TaxForm);
                // Validate the availability dates for 1098.
                foreach (var taxFormYear in taxForm1098Years)
                {
                    try
                    {
                        if (taxFormYear.Tf98yTaxYear == null)
                        {
                            throw new NullReferenceException("Tf98yTaxYear cannot be null.");
                        }
                        if (taxFormYear.Tf98yWebEnabled == null)
                        {
                            throw new NullReferenceException("Tf98yWebEnabled cannot be null.");
                        }
                        var available = true;
                        if (taxFormYear.Tf98yWebEnabled.ToUpper().Equals("Y"))
                        {
                            // If the tax form status doesn't exist, then the tax form is not available.
                            if (taxForm1098Status == null)
                            {
                                LogDataError("TaxFormStatus", "taxForm1098Status", taxForm1098Status);
                                available = false;
                            }
                            // If the tax form status year exists and is equal to the tax form year from taxForm1098Years, then we have additional evaluation.
                            if (taxForm1098Status.TfsTaxYear != null && taxForm1098Status.TfsTaxYear == taxFormYear.Tf98yTaxYear.ToString())
                            {
                                // If there is no gen date, the tax form is not available.
                                if (taxForm1098Status.TfsGenDate == null)
                                {
                                    available = false;
                                }
                                // If the tax form status exists and is "GEN", "MOD", or "UNF" then the tax form is not available.
                                if (taxForm1098Status.TfsStatus != null &&
                                    (taxForm1098Status.TfsStatus.ToUpper() == "GEN" ||
                                    taxForm1098Status.TfsStatus.ToUpper() == "MOD" ||
                                    taxForm1098Status.TfsStatus.ToUpper() == "UNF"))
                                {
                                    available = false;
                                }
                            }
                        }
                        else
                        {
                            // If the tax form year is specified as not web enabled, the tax form is not available.
                            available = false;
                        }
                        // Create a tax form availability object and add it to the tax form configuration.
                        configuration.AddAvailability(new TaxFormAvailability(taxFormYear.Tf98yTaxYear.ToString(), available));
                    }
                    catch (NullReferenceException e)
                    {
                        LogDataError("TaxForm1098Years", "contract", taxFormYear, e, e.Message);
                    }
                }
            }
            return configuration;
        }

        [Obsolete("Obsolete as of API 1.29.1. Use GetT4TaxFormAvailability2Async instead.")]
        private async Task<TaxFormConfiguration> GetT4TaxFormAvailabilityAsync()
        {
            TaxFormConfiguration configuration = new TaxFormConfiguration(TaxForms.FormT4);

            var qtdYtdParameterT4 = await DataReader.ReadRecordAsync<QtdYtdParameterT4>("HR.PARMS", "QTD.YTD.PARAMETER");
            if (qtdYtdParameterT4 != null)
            {
                // Validate the availability dates for T4.
                if (qtdYtdParameterT4.WebT4ParameterEntityAssociation != null)
                {
                    foreach (var dataContract in qtdYtdParameterT4.WebT4ParameterEntityAssociation)
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(dataContract.QypWebT4YearsAssocMember))
                                throw new ArgumentNullException("QypWebT4YearsAssocMember", "QypWebT4YearsAssocMember is required.");

                            if (!dataContract.QypWebT4AvailableDatesAssocMember.HasValue)
                                throw new ArgumentNullException("QypWebT4AvailableDatesAssocMember", "QypWebT4AvailableDatesAssocMember is required.");

                            configuration.AddAvailability(new TaxFormAvailability(dataContract.QypWebT4YearsAssocMember, dataContract.QypWebT4AvailableDatesAssocMember.Value));
                        }
                        catch (Exception e)
                        {
                            LogDataError("QypWebT4YearsAssocMember", "HR.PARMS - QTD.YTD.PARAMETER", dataContract, e, e.Message);
                        }
                    }
                }
            }

            return configuration;
        }

        #endregion

        #endregion



        ///////////////////////////////////////////////////////////////////////////////////
        ///                                                                             ///
        ///                                 CODE HOOKS                                  ///
        ///                                                                             ///
        ///////////////////////////////////////////////////////////////////////////////////

        #region CodeHooks

        /// <summary>
        /// Returns Output Data from Code Hooks Execution.
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="inputData"></param>
        /// <param name="resourceName"></param>
        /// <param name="resourceVersionNumber"></param>
        /// <param name="reportEthosApiErrors"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<string> GetOutputDataHookAsync(string columnName, string inputData, List<string> columnNames, List<string> columnValues, string resourceName, string resourceVersionNumber, bool reportEthosApiErrors = false, bool bypassCache = false)
        {
            var exception = new RepositoryException("Extensibility configuration errors.");

            List<string> inputDataList = new List<string>();
            inputDataList = inputData.Split(_VM).ToList();

            var allCodeHooks = await GetEthosExtensibilityCodeHooks(bypassCache);
            if (allCodeHooks == null || !allCodeHooks.Any())
            {
                return null;
            }
            var outputCodeHooks = allCodeHooks.Where(ch => ch.EdmcResourceName.Contains(resourceName.ToUpper())
                && ch.EdmcType.Equals("read", StringComparison.OrdinalIgnoreCase)
                && ch.EdmcFieldName.Contains(columnName.ToUpper())
                && (ch.EdmcFileName == null || !ch.EdmcFileName.Any()));

            foreach (var outputHook in outputCodeHooks)
            {
                bool matchingResourceVersion = false;
                foreach (var resource in outputHook.EdmcResourceEntityAssociation)
                {
                    if (!string.IsNullOrEmpty(resource.EdmcResourceVersionAssocMember))
                    {
                        if (resource.EdmcResourceNameAssocMember.Equals(resourceName, StringComparison.OrdinalIgnoreCase) && resource.EdmcResourceVersionAssocMember.Equals(resourceVersionNumber, StringComparison.OrdinalIgnoreCase))
                        {
                            matchingResourceVersion = true;
                        }
                    }
                    else
                    {
                        if (resource.EdmcResourceNameAssocMember.Equals(resourceName, StringComparison.OrdinalIgnoreCase))
                        {
                            matchingResourceVersion = true;
                        }
                    }
                }
                if (matchingResourceVersion)
                {
                    Dictionary<string, List<string>> columnDataRows = new Dictionary<string, List<string>>();
                    int index = 0;
                    var maxValues = columnValues.Count;
                    foreach (var column in columnNames)
                    {
                        if (index < maxValues)
                        {
                            if (!columnDataRows.ContainsKey(column))
                            {
                                columnDataRows.Add(column, columnValues[index].Replace(_TM, _VM).Replace(_XM, _SM).Split(_VM).ToList());
                            }
                        }
                    }

                    CodeBuilderObject inputObject = new CodeBuilderObject()
                    {
                        DataDictionary = columnDataRows,
                        DataValues = inputDataList,
                        SourceCode = outputHook.EdmcHookCode
                    };

                    var cacheName = CodeBuilderSupport.BuildCacheKey("OutputDataCodeHook", outputHook.Recordkey);
                    try
                    {
                        var result = await CodeBuilderSupport.CodeBuilderAsync(this, ContainsKey, GetOrAddToCache,
                            transactionInvoker, DataReader, cacheName, bypassCache, CacheTimeout, inputObject);

                        if (result != null && result.ErrorFlag)
                        {
                            var message = string.Format("Errors executing OutputData Hooks on '{2}' for resource '{0}' version number '{1}'", resourceName, resourceVersionNumber, columnName);
                            logger.Error(message);
                            exception.AddError(new RepositoryError("Data.Access", message));

                            foreach (var error in result.ErrorMessages)
                            {
                                message = error;
                                logger.Error(message);
                                if (reportEthosApiErrors)
                                {
                                    exception.AddError(new RepositoryError("Data.Access", message));
                                }
                            }
                            if (reportEthosApiErrors && exception != null && exception.Errors != null && exception.Errors.Any())
                            {
                                throw exception;
                            }
                        }

                        var outputData = string.Empty;
                        if (result != null && result.DataValues != null && result.DataValues.Any())
                        {
                            foreach (var data in result.DataValues)
                            {
                                if (!string.IsNullOrEmpty(outputData)) outputData = string.Concat(outputData, _VM);
                                outputData = string.Concat(outputData, data);
                            }
                        }
                        else
                        {
                            outputData = inputData;
                        }
                        inputData = outputData;
                    }
                    catch (RepositoryException ex)
                    {
                        var message = string.Format("Errors executing Variable Calculation Hooks on '{2}' for resource '{0}' version number '{1}'", resourceName, resourceVersionNumber, columnName);
                        logger.Error(message);
                        exception.AddError(new RepositoryError("Data.Access", message));
                        exception.AddError(new RepositoryError("Data.Access", ex.Errors.FirstOrDefault().Message));
                        if (reportEthosApiErrors && exception != null && exception.Errors != null && exception.Errors.Any())
                        {
                            throw exception;
                        }
                    }
                }
            }

            return inputData;
        }

        /// <summary>
        /// Returns Calculated Property Variable for insert into Json Data.
        /// </summary>
        /// <param name="resourceName"></param>
        /// <param name="resourceVersionNumber"></param>
        /// <param name="reportEthosApiErrors"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<IEnumerable<EthosExtensibleDataRow>> GetVariableCalculationHookAsync(string resourceName, string resourceVersionNumber, IList<EthosExtensibleDataRow> ethosExtensibleDataRows, bool reportEthosApiErrors = false, bool bypassCache = false)
        {
            var exception = new RepositoryException("Extensibility configuration errors.");

            List<EthosExtensibleDataRow> extensibleCalculatedRows = new List<EthosExtensibleDataRow>();

            var allCodeHooks = await GetEthosExtensibilityCodeHooks(bypassCache);
            if (allCodeHooks == null || !allCodeHooks.Any())
            {
                return null;
            }
            var calculatedProperties = allCodeHooks.Where(ch => ch.EdmcResourceName.Contains(resourceName.ToUpper()) && ch.EdmcType.Equals("calc", StringComparison.OrdinalIgnoreCase));

            foreach (var calcProperties in calculatedProperties)
            {
                bool matchingResourceVersion = false;
                foreach (var resource in calcProperties.EdmcResourceEntityAssociation)
                {
                    if (!string.IsNullOrEmpty(resource.EdmcResourceVersionAssocMember))
                    {
                        if (resource.EdmcResourceNameAssocMember.Equals(resourceName, StringComparison.OrdinalIgnoreCase) && resource.EdmcResourceVersionAssocMember.Equals(resourceVersionNumber, StringComparison.OrdinalIgnoreCase))
                        {
                            matchingResourceVersion = true;
                        }
                    }
                    else
                    {
                        if (resource.EdmcResourceNameAssocMember.Equals(resourceName, StringComparison.OrdinalIgnoreCase))
                        {
                            matchingResourceVersion = true;
                        }
                    }
                }
                if (matchingResourceVersion)
                {
                    Dictionary<string, List<string>> columnDataRows = ethosExtensibleDataRows.ToDictionary(edr => edr.ColleagueColumnName, edr => edr.ExtendedDataValue.Split(_VM).ToList());

                    CodeBuilderObject inputObject = new CodeBuilderObject()
                    {
                        DataDictionary = columnDataRows,
                        SourceCode = calcProperties.EdmcHookCode
                    };

                    var cacheName = CodeBuilderSupport.BuildCacheKey("VariableCalcuationRows", calcProperties.Recordkey);
                    try
                    {
                        var result = await CodeBuilderSupport.CodeBuilderAsync(this, ContainsKey, GetOrAddToCache,
                            transactionInvoker, DataReader, cacheName, bypassCache, CacheTimeout, inputObject);

                        if (result != null && result.ErrorFlag)
                        {
                            var message = string.Format("Errors executing Variable Calculation Hooks on '{2}' for resource '{0}' version number '{1}'", resourceName, resourceVersionNumber, string.Concat(calcProperties.EdmcJsonPath, calcProperties.EdmcJsonLabel));
                            logger.Error(message);
                            exception.AddError(new RepositoryError("Data.Access", message));

                            foreach (var error in result.ErrorMessages)
                            {
                                message = error;
                                logger.Error(message);
                                if (reportEthosApiErrors)
                                {
                                    exception.AddError(new RepositoryError("Data.Access", message));
                                }
                            }
                            if (reportEthosApiErrors && exception != null && exception.Errors != null && exception.Errors.Any())
                            {
                                throw exception;
                            }
                        }

                        string outputData = string.Empty;
                        if (result != null && result.DataValues != null && result.DataValues.Any())
                        {
                            foreach (var data in result.DataValues)
                            {
                                if (!string.IsNullOrEmpty(outputData)) outputData = string.Concat(outputData, _VM);
                                outputData = string.Concat(outputData, data);
                            }
                            var returnEthosDataRow = new EthosExtensibleDataRow(string.Concat("VAR", calcProperties.Recordkey),
                                "", calcProperties.EdmcJsonLabel, calcProperties.EdmcJsonPath, calcProperties.EdmcJsonPropertyType, outputData)
                            {
                                Description = calcProperties.EdmcDescription
                            };

                            extensibleCalculatedRows.Add(returnEthosDataRow);
                        }
                    }
                    catch (RepositoryException ex)
                    {
                        var message = string.Format("Errors executing Variable Calculation Hooks on '{2}' for resource '{0}' version number '{1}'", resourceName, resourceVersionNumber, string.Concat(calcProperties.EdmcJsonPath, calcProperties.EdmcJsonLabel));
                        logger.Error(message);
                        exception.AddError(new RepositoryError("Data.Access", message));
                        exception.AddError(new RepositoryError("Data.Access", ex.Errors.FirstOrDefault().Message));
                        if (reportEthosApiErrors && exception != null && exception.Errors != null && exception.Errors.Any())
                        {
                            throw exception;
                        }
                    }
                }
            }

            return extensibleCalculatedRows;
        }

        #endregion
    }
}