// Copyright 2017-2020 Ellucian Company L.P. and its affiliates.

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
using slf4net;
using Ellucian.Data.Colleague.DataContracts;
using System.Collections.ObjectModel;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.Common.Utility;
using Ellucian.Web.Http.Configuration;
using Ellucian.Dmi.Runtime;
using Ellucian.Colleague.Configuration;
using System.CodeDom.Compiler;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Entities;
using System.Reflection;
using Ellucian.Colleague.Domain.Base.Services;
using Ellucian.Colleague.Domain.Base;

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

        private IEnumerable<EdmExtVersions> _ethosExtensiblitySettingsList = null;
        /// <summary>
        /// Gets all of the Ethos Extensiblity settings stored on EDM.EXT.VERSIONS
        /// </summary>
        /// <param name="bypassCache">bool to determine if cache should be bypassed</param>
        /// <returns>List of DataContracts.EdmExtVersions</returns>
        public async Task<IEnumerable<EdmExtVersions>> GetEthosExtensibilityConfiguration(bool bypassCache = false)
        {
            if (_ethosExtensiblitySettingsList == null)
            {
                const string ethosExtensiblityCacheKey = "AllEthosExtensibiltySettings";

                if (bypassCache && ContainsKey(BuildFullCacheKey(ethosExtensiblityCacheKey)))
                {
                    ClearCache(new List<string> { ethosExtensiblityCacheKey });
                }

                var ethosExtensiblitySettingsList =
                    await GetOrAddToCacheAsync<List<EdmExtVersions>>(ethosExtensiblityCacheKey,
                        async () => (await DataReader.BulkReadRecordAsync<EdmExtVersions>("EDM.EXT.VERSIONS", "")).ToList(), CacheTimeout);

                _ethosExtensiblitySettingsList = ethosExtensiblitySettingsList;
            }
            return _ethosExtensiblitySettingsList;
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
                catch (Exception)
                {
                    // If we are missing code hooks table, then do not throw an exception.
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
                ClearCache(new List<string> { ethosExtensiblityCacheKey });
            }

            List<string> edmeVersions = new List<string>();
            if (customOnly)
            {
                var extensions = await DataReader.BulkReadRecordAsync<EdmExtensions>("EDM.EXTENSIONS", "WITH EDME.PRIMARY.GUID.SOURCE NE ''");
                foreach (var ext in extensions)
                {
                    edmeVersions.AddRange(ext.EdmeVersions);
                }

            }
            var ethosExtensiblitySettingsList =
                await GetOrAddToCacheAsync<List<EdmExtVersions>>(ethosExtensiblityCacheKey,
                    async () => (await DataReader.BulkReadRecordAsync<EdmExtVersions>("EDM.EXT.VERSIONS", edmeVersions.Any() ? edmeVersions.ToArray() : null,
                        false)).ToList(), CacheTimeout);

            foreach (var ethosExtensiblitySetting in ethosExtensiblitySettingsList)
            {
                var extendedEthosConfiguration = (await GetExtendedEthosConfigurationByResource(ethosExtensiblitySetting.EdmvResourceName, ethosExtensiblitySetting.EdmvVersionNumber,
                        ethosExtensiblitySetting.EdmvResourceName.ToUpperInvariant()));
                if (extendedEthosConfiguration != null)
                {
                    try
                    {
                        var depKey = string.Concat(extendedEthosConfiguration.ApiResourceName.ToUpper() + "*" + extendedEthosConfiguration.ApiVersionNumber);
                        if (depKey != null)
                        {
                            var depData = await DataReader.BulkReadRecordAsync<EdmDepNotices>("EDM.DEP.NOTICES", new string[] { depKey }, false);
                            if (depData != null && depData.Any())
                            {
                                extendedEthosConfiguration.DeprecationDate = depData[0].EdmpDeprecationDate;
                                extendedEthosConfiguration.DeprecationNotice = depData[0].EdmpDeprecationNotice.Replace(_VM, ' ');
                                extendedEthosConfiguration.SunsetDate = depData[0].EdmpSunsetDate;
                            }
                        }

                        var extensions = await DataReader.BulkReadRecordAsync<EdmExtensions>("EDM.EXTENSIONS",
                            string.Format("WITH EDME.RESOURCE.NAME EQ '{0}'", extendedEthosConfiguration.ApiResourceName), false);
                        if (extensions != null && extensions.Any())
                        {
                            extendedEthosConfiguration.HttpMethodsSupported = extensions[0].EdmeHttpMethods;
                        }
                    }
                    catch (Exception ex)
                    {
                        //do not throw error on supplemental data.
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
        public async Task<string> GetEthosExtensibilityResourceDefaultVersion(string resourceName, bool bypassCache = false)
        {
            string defaultVersion = string.Empty;
            try
            {
                var extendConfigData = (await GetEthosExtensibilityConfiguration(bypassCache)).ToList();
                var matchingExtendedConfigData = extendConfigData.Where(e =>
                    e.EdmvResourceName.Equals(resourceName, StringComparison.OrdinalIgnoreCase));
                var availableVersions = matchingExtendedConfigData.Where(e => !string.IsNullOrEmpty(e.EdmvVersionNumber)).Select(e => e.EdmvVersionNumber).OrderBy(n => n.Split('.')[0]).ToList();
                defaultVersion = availableVersions.LastOrDefault();
                if (string.IsNullOrEmpty(defaultVersion))
                {
                    defaultVersion = "1.0.0";
                }
            }
            catch (Exception e)
            {
                logger.Error(e, string.Concat("Retreiving Ethos Extended Confguration has failed for resource : ", resourceName));
            }

            return defaultVersion;
        }

        /// <summary>
        /// Gets the extended configuration available on a resource, returns null if there are none
        /// </summary>
        /// <param name="resourceName">name of the resource (api) </param>
        /// <param name="resourceVersionNumber">version number of ther resource</param>
        /// <param name="extendedSchemaResourceId">extended schema identifier</param>
        /// <returns> extended configuration if available. Returns null if none available or none configured</returns>
        public async Task<EthosExtensibleData> GetExtendedEthosConfigurationByResource(string resourceName, string resourceVersionNumber, string extendedSchemaResourceId, bool bypassCache = false)
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
                            var extendResourceConfigData = (await GetEthosApiConfiguration(bypassCache)).ToList();
                            var apiConfiguration = extendResourceConfigData.FirstOrDefault(ex => ex.EdmeResourceName == resourceName);

                            var extendVersionConfigData = (await GetEthosExtensibilityConfiguration(bypassCache)).ToList();

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


                            //list of linked column config data for datetime columns, exlcuding date or time only ones.
                            var linkedColumnDetails = matchingExtendedConfigData.EdmvColumnsEntityAssociation
                                .Where(l => !string.IsNullOrEmpty(l.EdmvDateTimeLinkAssocMember) &&
                                            !l.EdmvJsonPropertyTypeAssocMember.Equals("date", StringComparison.OrdinalIgnoreCase)).ToList();


                            //loop through and add each row
                            foreach (var edmvColumn in matchingExtendedConfigData.EdmvColumnsEntityAssociation)
                            {
                                //if the datetime link assoc has a value it is part of a pair
                                //only add the value that is marked as the datetime
                                if (!string.IsNullOrEmpty(edmvColumn.EdmvDateTimeLinkAssocMember))
                                {
                                    if (!edmvColumn.EdmvConversionAssocMember.StartsWith("D", StringComparison.OrdinalIgnoreCase)) continue;

                                    var row = new EthosExtensibleDataRow(edmvColumn.EdmvColumnNameAssocMember, edmvColumn.EdmvFileNameAssocMember,
                                        edmvColumn.EdmvJsonLabelAssocMember, edmvColumn.EdmvJsonPathAssocMember,
                                        edmvColumn.EdmvJsonPropertyTypeAssocMember, "", edmvColumn.EdmvLengthAssocMember)
                                    {
                                        associationController = edmvColumn.EdmvAssociationControllerAssocMember,
                                        transType = edmvColumn.EdmvTransTypeAssocMember,
                                        databaseUsageType = edmvColumn.EdmvDatabaseUsageTypeAssocMember
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
                                            associationController = timeConfig.EdmvAssociationControllerAssocMember,
                                            transType = timeConfig.EdmvTransTypeAssocMember,
                                            databaseUsageType = timeConfig.EdmvDatabaseUsageTypeAssocMember
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
                                        associationController = edmvColumn.EdmvAssociationControllerAssocMember,
                                        transType = edmvColumn.EdmvTransTypeAssocMember,
                                        databaseUsageType = edmvColumn.EdmvDatabaseUsageTypeAssocMember
                                    };

                                    extendedDataConfigReturn.AddItemToExtendedData(row);
                                }

                                // Add Filter Criteria to configuration
                                EdmSelectCriteria selectCriteria = null;
                                if (!string.IsNullOrEmpty(edmvColumn.EdmvFilterCriteriaAssocMember))
                                {
                                    var selectConfigData = (await GetEthosApiSelectCriteria(bypassCache)).ToList();
                                    selectCriteria = selectConfigData.FirstOrDefault(sc => sc.Recordkey == edmvColumn.EdmvFilterCriteriaAssocMember);
                                }
                                else
                                {
                                    // If we are pointing to a different table, then we can't select
                                    // from the primary table and the selection criteria is too complicated
                                    // to simply select with column eqal to value.
                                    if (string.IsNullOrEmpty(edmvColumn.EdmvSecColumnNameAssocMember))
                                    {
                                        if (!edmvColumn.EdmvFileNameAssocMember.EndsWith("VALCODES"))
                                        {
                                            selectCriteria = new EdmSelectCriteria()
                                            {
                                                EdmsSelectFileName = edmvColumn.EdmvFileNameAssocMember,
                                                EdmsSelectColumnName = edmvColumn.EdmvJsonLabelAssocMember,
                                                EdmsSelectEntityAssociation = new List<EdmSelectCriteriaEdmsSelect>()
                                                {
                                                    new EdmSelectCriteriaEdmsSelect()
                                                    {
                                                        EdmsSelectColumnAssocMember = edmvColumn.EdmvColumnNameAssocMember,
                                                        EdmsSelectConnectorAssocMember = "WITH",
                                                        EdmsSelectOperAssocMember = "EQ",
                                                        EdmsSelectValueAssocMember = '"' + edmvColumn.EdmvJsonLabelAssocMember + '"'
                                                    }
                                                },
                                                EdmsSortEntityAssociation = new List<EdmSelectCriteriaEdmsSort>()
                                            };
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
                                                    EdmsSelectColumnName = edmvColumn.EdmvJsonLabelAssocMember,
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
                                                            EdmsSelectColumnAssocMember = edmvColumn.EdmvColumnNameAssocMember,
                                                            EdmsSelectConnectorAssocMember = "WHEN",
                                                            EdmsSelectOperAssocMember = "EQ",
                                                            EdmsSelectValueAssocMember = '"' + edmvColumn.EdmvJsonLabelAssocMember + '"'
                                                        }
                                                    },
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
                                    var filterRow = new EthosExtensibleDataFilter(edmvColumn.EdmvColumnNameAssocMember, edmvColumn.EdmvFileNameAssocMember,
                                            edmvColumn.EdmvJsonLabelAssocMember, edmvColumn.EdmvJsonPathAssocMember,
                                            edmvColumn.EdmvJsonPropertyTypeAssocMember, new List<string>());

                                    filterRow.SelectFileName = selectCriteria.EdmsSelectFileName;
                                    filterRow.SelectSubroutineName = selectCriteria.EdmsSelectSubroutine;
                                    filterRow.SavingField = selectCriteria.EdmsSavingField;
                                    filterRow.SavingOption = selectCriteria.EdmsSavingOption;
                                    filterRow.SelectColumnName = selectCriteria.EdmsSelectColumnName;
                                    filterRow.SelectRules = selectCriteria.EdmsSelectRules;
                                    filterRow.SelectParagraph = selectCriteria.EdmsSelectParagraph;

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

                            // Add Named Queries to configuration
                            if (matchingExtendedConfigData.EdmvNamedQueries != null && matchingExtendedConfigData.EdmvNamedQueries.Any())
                            {
                                foreach (var edmQuery in matchingExtendedConfigData.EdmvNamedQueries)
                                {
                                    var queryConfigData = (await GetEthosApiNamedQueries(bypassCache)).ToList();
                                    var queryData = queryConfigData.FirstOrDefault(sc => sc.Recordkey == edmQuery);
                                    var selectConfigData = (await GetEthosApiSelectCriteria(bypassCache)).ToList();
                                    var selectCriteria = selectConfigData.FirstOrDefault(sc => sc.Recordkey == queryData.EdmqSelectCriteria);
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

        private IEnumerable<EdmExtensions> _ethosExtensibilitySettings = null;
        /// <summary>
        /// Gets all of the Ethos API builder settings stored on EDM.EXTENSIONS
        /// </summary>
        /// <param name="bypassCache">bool to determine if cache should be bypassed</param>
        /// <returns>List of DataContracts.EdmExtensions</returns>
        private async Task<IEnumerable<EdmExtensions>> GetEthosApiConfiguration(bool bypassCache = false)
        {
            if (_ethosExtensibilitySettings == null)
            {
                const string ethosExtensiblityCacheKey = "AllEthosApiBuilderConfigurationSettings";

                if (bypassCache && ContainsKey(BuildFullCacheKey(ethosExtensiblityCacheKey)))
                {
                    ClearCache(new List<string> { ethosExtensiblityCacheKey });
                }

                var ethosExtensiblitySettingsList =
                    await GetOrAddToCacheAsync(ethosExtensiblityCacheKey,
                        async () => (await DataReader.BulkReadRecordAsync<EdmExtensions>("EDM.EXTENSIONS", "")).ToList(), CacheTimeout);

                _ethosExtensibilitySettings = ethosExtensiblitySettingsList;
            }
            return _ethosExtensibilitySettings;
        }

        private IEnumerable<EdmSelectCriteria> _ethosSelectionCriteria = null;
        /// <summary>
        /// Gets all of the Ethos API builder settings stored on EDM.SELECT.CRITERIA
        /// </summary>
        /// <param name="bypassCache">bool to determine if cache should be bypassed</param>
        /// <returns>List of DataContracts.EdmExtensions</returns>
        private async Task<IEnumerable<EdmSelectCriteria>> GetEthosApiSelectCriteria(bool bypassCache = false)
        {
            if (_ethosSelectionCriteria == null)
            {
                const string ethosExtensiblityCacheKey = "AllEthosApiSelectCriteriaConfigurationSettings";

                if (bypassCache && ContainsKey(BuildFullCacheKey(ethosExtensiblityCacheKey)))
                {
                    ClearCache(new List<string> { ethosExtensiblityCacheKey });
                }

                var ethosExtensiblitySettingsList =
                    await GetOrAddToCacheAsync<List<EdmSelectCriteria>>(ethosExtensiblityCacheKey,
                        async () => (await DataReader.BulkReadRecordAsync<EdmSelectCriteria>("EDM.SELECT.CRITERIA", "")).ToList(), CacheTimeout);

                _ethosSelectionCriteria = ethosExtensiblitySettingsList;
            }
            return _ethosSelectionCriteria;
        }

        private IEnumerable<EdmQueries> _ethosExtensibilityQueries = null;
        /// <summary>
        /// Gets all of the Ethos API builder settings stored on EDM.QUERIES
        /// </summary>
        /// <param name="bypassCache">bool to determine if cache should be bypassed</param>
        /// <returns>List of DataContracts.EdmExtensions</returns>
        private async Task<IEnumerable<EdmQueries>> GetEthosApiNamedQueries(bool bypassCache = false)
        {
            if (_ethosExtensibilityQueries == null)
            {
                const string ethosExtensiblityCacheKey = "AllEthosApiNamedQueriesConfigurationSettings";

                if (bypassCache && ContainsKey(BuildFullCacheKey(ethosExtensiblityCacheKey)))
                {
                    ClearCache(new List<string> { ethosExtensiblityCacheKey });
                }

                var ethosExtensiblitySettingsList =
                    await GetOrAddToCacheAsync<List<EdmQueries>>(ethosExtensiblityCacheKey,
                        async () => (await DataReader.BulkReadRecordAsync<EdmQueries>("EDM.QUERIES", "")).ToList(), CacheTimeout);

                _ethosExtensibilityQueries = ethosExtensiblitySettingsList;
            }
            return _ethosExtensibilityQueries;
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
                            var extendConfigData = (await GetEthosApiConfiguration(bypassCache)).ToList();
                            var apiConfiguration = extendConfigData.FirstOrDefault(ex => ex.EdmeResourceName == resourceName);

                            if (!string.IsNullOrEmpty(apiConfiguration.EdmeSelectCriteria))
                            {
                                var selectConfigData = (await GetEthosApiSelectCriteria(bypassCache)).ToList();
                                selectCriteria = selectConfigData.FirstOrDefault(sc => sc.Recordkey == apiConfiguration.EdmeSelectCriteria);
                            }
                            if (apiConfiguration != null)
                            {
                                var ethosApiConfiguration = new EthosApiConfiguration()
                                {
                                    ResourceName = apiConfiguration.EdmeResourceName,
                                    PrimaryEntity = apiConfiguration.EdmePrimaryEntity.EndsWith("VALCODES")
                                        && !string.IsNullOrEmpty(apiConfiguration.EdmePrimaryApplication)
                                        && !string.IsNullOrEmpty(apiConfiguration.EdmePrimaryTableName)
                                        ? (await ValidateValcodeTable(apiConfiguration.EdmePrimaryApplication, apiConfiguration.EdmePrimaryTableName))
                                        : apiConfiguration.EdmePrimaryEntity,
                                    PrimaryApplication = apiConfiguration.EdmePrimaryApplication,
                                    PrimaryTableName = apiConfiguration.EdmePrimaryTableName,
                                    PrimaryGuidSource = apiConfiguration.EdmePrimaryGuidSource,
                                    PrimaryGuidDbType = apiConfiguration.EdmePrimaryGuidDbType,
                                    PrimaryGuidFileName = apiConfiguration.EdmePrimaryGuidFileName.EndsWith("VALCODES")
                                        && !string.IsNullOrEmpty(apiConfiguration.EdmePrimaryApplication)
                                        && !string.IsNullOrEmpty(apiConfiguration.EdmePrimaryTableName)
                                        ? (await ValidateValcodeTable(apiConfiguration.EdmePrimaryApplication, apiConfiguration.EdmePrimaryTableName))
                                        : apiConfiguration.EdmePrimaryGuidFileName,
                                    PageLimit = apiConfiguration.EdmePageLimit
                                };

                                ethosApiConfiguration.HttpMethods = new List<EthosApiSupportedMethods>();
                                foreach (var method in apiConfiguration.EdmeMethodsEntityAssociation)
                                {
                                    if (!string.IsNullOrEmpty(method.EdmeHttpMethodsAssocMember))
                                    {
                                        ethosApiConfiguration.HttpMethods.Add(
                                            new EthosApiSupportedMethods(method.EdmeHttpMethodsAssocMember,
                                            method.EdmeHttpPermissionsAssocMember)
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

            if (ethosApiConfigurationByResource == null || string.IsNullOrEmpty(ethosApiConfigurationByResource.PrimaryEntity))
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
        public async Task<IEnumerable<EthosExtensibleData>> GetExtendedEthosDataByResource(string resourceName, string resourceVersionNumber, string extendedSchemaResourceId, IEnumerable<string> resourceIds, bool reportEthosApiErrors = false, bool bypassCache = false)
        {
            var exception = new RepositoryException("Extensibility configuration errors.");

            var extendConfigData = (await GetEthosExtensibilityConfiguration(bypassCache)).ToList();

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

            var retConfigData = new List<EthosExtensibleData>();
            if (resourceIds == null || !resourceIds.Any())
            {
                return retConfigData;
            }

            //crate guidlookup array to get primary key info
            var guidLookupArray = resourceIds.Where(i => !string.IsNullOrWhiteSpace(i)).Select(r => new GuidLookup(r)).ToArray();

            //get coll primary key info and if none return exit with empty list
            var idDict = await DataReader.SelectAsync(guidLookupArray);
            if (idDict == null || !idDict.Any())
            {
                if (reportEthosApiErrors)
                {
                    var message = string.Format("Record Keys for GUIDs not found for resource '{0}' version number '{1}'", resourceName, resourceVersionNumber);
                    logger.Error(message);
                    exception.AddError(new RepositoryError("Bad.Data", message));
                    throw exception;
                }
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

            var allColumnData = new Dictionary<string, Dictionary<string, string>>();
            var colleagueSecondaryKeys = new Dictionary<string, string>();

            // For now, the use of the CTX is still experimental though most likely
            // it will continue to be used.  However, if we don't support virtual fields
            // then we won't necessarily need the CTX.  We would however need to add
            // reads of all secondary data columns/files using the pointers defined.
            // So, for now (as of 04/14/2020 SRM) we will leave in the old code and
            // force it to use the CTX.

            bool useCtx = true;
            if (useCtx)
            {
                var request = new GetEthosExtendedDataRequest()
                {
                    Guids = resourceIds.ToList(),
                    ResourceName = resourceName,
                    Version = resourceVersionNumber
                };
                try
                {
                    var response = await transactionInvoker.ExecuteAsync<GetEthosExtendedDataRequest, GetEthosExtendedDataResponse>(request);
                    if (response.Error || (response.GetEthosExtendDataErrors != null && response.GetEthosExtendDataErrors.Any()))
                    {
                        var message = string.Format("Extensibility configuration errors for resource '{0}' version number '{1}'", resourceName, resourceVersionNumber);
                        logger.Error(message);
                        exception.AddError(new RepositoryError("Data.Access", message));

                        foreach (var error in response.GetEthosExtendDataErrors)
                        {
                            message = string.Concat(error.ErrorCodes, " - ", error.ErrorMessages);
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
                        return retConfigData;
                    }

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
                                var inputData = columnValues.ElementAt(index).Replace(_TM, _VM).Replace(_XM, _SM);
                                var outputData = await GetOutputDataHookAsync(columnName, inputData, columnNames, columnValues, resourceName, resourceVersionNumber, reportEthosApiErrors, bypassCache);

                                if (!string.IsNullOrEmpty(outputData))
                                {
                                    columnDataItem.Add(columnName, outputData);
                                }
                                else
                                {
                                    columnDataItem.Add(columnName, inputData);
                                }
                                index++;
                            }
                            if (!string.IsNullOrEmpty(resp.ResourceSecondaryKeys))
                            {
                                if (!colleagueSecondaryKeys.ContainsKey(resp.ResourceGuids))
                                {
                                    colleagueSecondaryKeys.Add(resp.ResourceGuids, resp.ResourceSecondaryKeys);
                                }
                            }
                            //if the allColumndata contains the key column data has already been added for this record 
                            //so the new column data must be combined with the existing column data
                            if (allColumnData.ContainsKey(resp.ResourceGuids))
                            {
                                var currentColumnDictionary = allColumnData[resp.ResourceGuids];
                                var unionOfResults = currentColumnDictionary.Union(columnDataItem)
                                    .ToDictionary(k => k.Key, v => v.Value);
                                allColumnData[resp.ResourceGuids] = unionOfResults;
                            }
                            else //key didn't exist yet so just add the guid as the key with the current column data set
                            {
                                allColumnData.Add(resp.ResourceGuids, columnDataItem);
                            }
                        }
                    }
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
            else
            {
                //get the filenames from the config data, extended data can come from any file keyed to the main entity
                var fileNameStrList = matchingExtendedConfigData.EdmvColumnsEntityAssociation.Select(e => e.EdmvFileNameAssocMember).ToList().Distinct();
                var colleagueFileAndKeys = new Dictionary<string, List<string>>();

                foreach (var guidLkup in idDict)
                {
                    // Found in some cases, the guid lookup fails to return a GUID record causing object reference error.
                    if (guidLkup.Value != null && !string.IsNullOrEmpty(guidLkup.Value.Entity) && !string.IsNullOrEmpty(guidLkup.Value.PrimaryKey))
                    {
                        if (colleagueFileAndKeys.ContainsKey(guidLkup.Value.Entity))
                        {
                            colleagueFileAndKeys[guidLkup.Value.Entity].Add(guidLkup.Value.PrimaryKey);
                        }
                        else //key didn't exist yet so just add the guid as the key with the current column data set
                        {
                            colleagueFileAndKeys.Add(guidLkup.Value.Entity, new List<string>() { guidLkup.Value.PrimaryKey });
                        }
                        if (!string.IsNullOrEmpty(guidLkup.Value.SecondaryKey))
                        {
                            if (!colleagueSecondaryKeys.ContainsKey(guidLkup.Key))
                            {
                                colleagueSecondaryKeys.Add(guidLkup.Key, guidLkup.Value.SecondaryKey);
                            }
                        }
                    }
                }

                var fileSuiteInstance = string.Empty;
                foreach (var fileAndKeys in colleagueFileAndKeys)
                {
                    fileSuiteInstance = await GetEthosFileSuiteInstance(fileAndKeys.Key);
                    //get the extended colum data from each file and put into single dictionary
                    foreach (var fileName in fileNameStrList)
                    {
                        //don't process empty file name
                        if (string.IsNullOrEmpty(fileName))
                        {
                            var message = string.Concat("Extensibility configuration does not have a filename for resource ", resourceName, " version number ", resourceVersionNumber);
                            logger.Error(message);
                            exception.AddError(new RepositoryError("Data.Access", message));
                            continue;
                        }
                        //get the column names for the file we are going to get the data from
                        var fileColumns = matchingExtendedConfigData.EdmvColumnsEntityAssociation
                            .Where(e => e.EdmvFileNameAssocMember.Equals(fileName, StringComparison.OrdinalIgnoreCase)
                            && !e.EdmvDatabaseUsageTypeAssocMember.Equals("K", StringComparison.OrdinalIgnoreCase))
                            .Select(e => e.EdmvColumnNameAssocMember).Distinct().ToList();

                        //get the Key column names for the file we are going to get the data from
                        var keyColumns = matchingExtendedConfigData.EdmvColumnsEntityAssociation
                            .Where(e => e.EdmvFileNameAssocMember.Equals(fileName, StringComparison.OrdinalIgnoreCase)
                            && e.EdmvDatabaseUsageTypeAssocMember.Equals("K", StringComparison.OrdinalIgnoreCase))
                            .Select(e => e.EdmvColumnNameAssocMember).Distinct().ToList();

                        var additionalColumns = matchingExtendedConfigData.EdmvColumnsEntityAssociation
                            .Where(e => e.EdmvFileNameAssocMember.Equals(fileName, StringComparison.OrdinalIgnoreCase)
                            && !string.IsNullOrEmpty(e.EdmvAssociationControllerAssocMember))
                            .Select(e => e.EdmvAssociationControllerAssocMember).Distinct().ToList();

                        fileColumns.AddRange(additionalColumns);
                        var fileColumnNames = fileColumns.Distinct().ToArray();

                        //var fileSuiteFileName = await GetEthosFileSuiteFileNameAsync(fileSuiteYear, fileName);
                        var physicalFileName = fileName;
                        if (!string.IsNullOrEmpty(fileSuiteInstance) && await IsEthosFileSuiteTemplateFile(fileName))
                        {
                            physicalFileName = await GetEthosFileSuiteFileNameAsync(fileName, fileSuiteInstance);
                        }
                        var collIdsByFileName = fileAndKeys.Value.Distinct().ToArray();

                        //get the colleague keys from the select data - This will handle if they are co-files (temp fix until the "primary" file can be identified)
                        var colleagueKeys = idDict.Where(id => id.Value != null).Select(i => i.Value.PrimaryKey).Distinct().ToArray();

                        var colleagueIdsToQueryWith = collIdsByFileName.Any() ? collIdsByFileName : colleagueKeys;

                        //We may have only key columns and no actual data columns.
                        if (fileColumnNames != null && fileColumnNames.Any())
                        {
                            //data for the columns matching to the file for the set of keys
                            var currentFileColumnData = await DataReader.BatchReadRecordColumnsAsync(physicalFileName, colleagueIdsToQueryWith, fileColumnNames);

                            //go through each columndata result to add the key and column data to the single collection
                            foreach (var columnDataItem in currentFileColumnData)
                            {
                                //get the guidlookupresult for the colleague key and primary filename
                                //there can be multiple guids for the same colleague id when there are multiple primary files involved on the API
                                var guidForColumnData = idDict.Where(i => i.Value != null && i.Value.PrimaryKey.Equals(columnDataItem.Key) &&
                                    i.Value.Entity.Equals(physicalFileName, StringComparison.OrdinalIgnoreCase));

                                if (guidForColumnData == null || !guidForColumnData.Any())
                                {
                                    //get the guids for the colleague key, there can be multiple guids for the same colleague id
                                    guidForColumnData = idDict.Where(i => i.Value != null && i.Value.PrimaryKey.Equals(columnDataItem.Key)).ToList();
                                }

                                if (guidForColumnData.Any())
                                {
                                    foreach (var collIdKeyPair in guidForColumnData)
                                    {
                                        //if the allColumndata contains the key column data has already been added for this record 
                                        //so it the new column data must be combined with the existing column data
                                        if (allColumnData.ContainsKey(collIdKeyPair.Key))
                                        {
                                            var currentColumnDictionary = allColumnData[collIdKeyPair.Key];
                                            var unionOfResults = currentColumnDictionary.Union(columnDataItem.Value)
                                                .ToDictionary(k => k.Key, v => v.Value);
                                            allColumnData[collIdKeyPair.Key] = unionOfResults;
                                        }
                                        else //key didn't exist yet so just add the guid as the key with the current column data set
                                        {
                                            allColumnData.Add(collIdKeyPair.Key, columnDataItem.Value);
                                        }
                                    }
                                }
                            }
                        }

                        // Add all data coming from the Colleague record keys to the allColumnData dictionary.
                        if (keyColumns != null && keyColumns.Any())
                        {
                            foreach (var keyColumnName in keyColumns)
                            {
                                foreach (var collId in colleagueIdsToQueryWith)
                                {
                                    var keyDict = new Dictionary<string, string>();
                                    keyDict.Add(keyColumnName, collId);
                                    //get the guidlookupresult for the colleague key and primary filename
                                    //there can be multiple guids for the same colleague id when there are multiple primary files involved on the API
                                    var guidForColumnData = idDict.Where(i => i.Value != null && i.Value.PrimaryKey.Equals(collId) &&
                                        i.Value.Entity.Equals(physicalFileName, StringComparison.OrdinalIgnoreCase));

                                    if (guidForColumnData == null || !guidForColumnData.Any())
                                    {
                                        //get the guids for the colleague key, there can be multiple guids for the same colleague id
                                        guidForColumnData = idDict.Where(i => i.Value != null && i.Value.PrimaryKey.Equals(collId)).ToList();
                                    }

                                    if (guidForColumnData.Any())
                                    {
                                        foreach (var collIdKeyPair in guidForColumnData)
                                        {
                                            //if the allColumndata contains the key column data has already been added for this record 
                                            //so it the new column data must be combined with the existing column data
                                            if (allColumnData.ContainsKey(collIdKeyPair.Key))
                                            {
                                                var currentColumnDictionary = allColumnData[collIdKeyPair.Key];
                                                var unionOfResults = currentColumnDictionary.Union(keyDict)
                                                    .ToDictionary(k => k.Key, v => v.Value);
                                                allColumnData[collIdKeyPair.Key] = unionOfResults;
                                            }
                                            else //key didn't exist yet so just add the guid as the key with the current column data set
                                            {
                                                allColumnData.Add(collIdKeyPair.Key, keyDict);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            //list of linked column config data for datetime columns, exlcuding date or time only ones.
            var linkedColumnDetails = matchingExtendedConfigData.EdmvColumnsEntityAssociation
                .Where(l => !string.IsNullOrEmpty(l.EdmvDateTimeLinkAssocMember) &&
                            !l.EdmvJsonPropertyTypeAssocMember.Equals("date", StringComparison.OrdinalIgnoreCase)).ToList();

            foreach (var cData in allColumnData)
            {
                var newEthosThing = new EthosExtensibleData(resourceName, resourceVersionNumber, matchingExtendedConfigData.EdmvExtendedSchemaType, cData.Key, colleagueTimeZone);

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
                        var message = string.Concat("Extensibility configuration does not have a column configuration for ", colKeyValuePair.Key, " for resource ", resourceName);
                        logger.Error(message);
                        //exception.AddError(new RepositoryError("Data.Access", message));
                        continue;
                    }
                    // Determine the Colleague Data value to be returned with each record key.
                    if (!string.IsNullOrEmpty(columnConfigDetails.EdmvAssociationControllerAssocMember) && columnConfigDetails.EdmvDatabaseUsageTypeAssocMember.ToUpper() == "A")
                    {
                        // If we are dealing with a Valcode table, we need to find the guid key and
                        // compare that to the resource key to find the associated value in a multi-valued
                        // associated data element.
                        var associationColumn = matchingExtendedConfigData.EdmvColumnsEntityAssociation.FirstOrDefault(m =>
                            m.EdmvColumnNameAssocMember.Equals(columnConfigDetails.EdmvAssociationControllerAssocMember));

                        var secondaryKeyValue = cData.Value.FirstOrDefault(cdv => cdv.Key == columnConfigDetails.EdmvAssociationControllerAssocMember);
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
                                            catch
                                            {
                                                // Ignore for now.
                                            }
                                        }
                                        if (secondaryKeyListValue == secondaryKey)
                                        {
                                            var newValue = colleagueValue.Split(_VM);
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
                        // If we have comments or text, then convert value mark to spaces.
                        if (colleagueValue.Contains(_VM))
                        {
                            if (columnConfigDetails.EdmvDatabaseUsageTypeAssocMember.ToUpper() == "C" || columnConfigDetails.EdmvDatabaseUsageTypeAssocMember.ToUpper() == "T")
                            {
                                var newValue = colKeyValuePair.Value.Split(_VM);
                                colleagueValue = "";
                                foreach (var value in newValue)
                                {
                                    colleagueValue = string.Concat(colleagueValue, " ", value);
                                }
                            }
                            else
                            {
                                colleagueValue = colKeyValuePair.Value;
                            }
                        }
                    }
                    if (string.IsNullOrEmpty(colleagueValue))
                    {
                        continue;
                    }
                    if (columnConfigDetails.EdmvDatabaseUsageTypeAssocMember.Equals("K", StringComparison.OrdinalIgnoreCase) && colleagueValue.Contains("*"))
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

                    // if this is a match to an item i the linkedColumnDetail this means it is a linked column and needs to be treated different
                    if (linkedColumnDetails.Any(c => c.EdmvColumnNameAssocMember.Equals(colKeyValuePair.Key, StringComparison.OrdinalIgnoreCase)))
                    {
                        var jsonPropType = columnConfigDetails.EdmvConversionAssocMember.StartsWith("D", StringComparison.OrdinalIgnoreCase)
                            ? "date" : "time";

                        linkedColumnsTuple.Add(new Tuple<string, string, string, string>(jsonPropType,
                            columnConfigDetails.EdmvDateTimeLinkAssocMember, colKeyValuePair.Key,
                            colleagueValue));
                    }
                    else //else this is just a single value column
                    {
                        var row = new EthosExtensibleDataRow(columnConfigDetails.EdmvColumnNameAssocMember, columnConfigDetails.EdmvFileNameAssocMember,
                            columnConfigDetails.EdmvJsonLabelAssocMember, columnConfigDetails.EdmvJsonPathAssocMember,
                            columnConfigDetails.EdmvJsonPropertyTypeAssocMember, colleagueValue)
                        {
                            associationController = columnConfigDetails.EdmvAssociationControllerAssocMember,
                            transType = columnConfigDetails.EdmvTransTypeAssocMember,
                            databaseUsageType = columnConfigDetails.EdmvDatabaseUsageTypeAssocMember

                        }; ;

                        newEthosThing.AddItemToExtendedData(row);
                    }
                }
                try
                {
                    var datetimeExtensions = ConvertColleageDateAndTimeValues(linkedColumnsTuple, linkedColumnDetails);

                    datetimeExtensions.ForEach(e => newEthosThing.AddItemToExtendedData(e));
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
        /// Adds the Metadata objects to the extended data configuration. 
        /// </summary>
        /// <param name="matchingExtendedConfigData">Configuration for extended data.</param>
        /// <param name="idDict">dictionary containing the ids for the resources in guidlookup form</param>
        /// <returns>List of extensions with all of the metadata objects defined.</returns>
        private async Task<EdmExtVersions> AddEthosMetadataConfigurationData(EdmExtVersions matchingExtendedConfigData, Dictionary<string, GuidLookupResult> idDict, bool bypassCache = false)
        {
            var matchingColumnConfigDetails = new List<EdmExtVersionsEdmvColumns>();

            var allFileNames = new List<string>();
            var fileNamesForGuids = idDict.Where(i => i.Value != null && i.Value.Entity != null).Select(i => i.Value.Entity).Distinct().ToList();
            foreach (var fileName in fileNamesForGuids)
            {
                // Found in some cases, the guid lookup fails to return a GUID record causing object reference error.
                if (fileName != null)
                {
                    var templateFileName = await GetEthosFileSuiteTemplate(fileName, bypassCache);
                    if (!allFileNames.Contains(templateFileName))
                    {
                        allFileNames.Add(templateFileName);
                    }
                }
            }

            foreach (var fileName in allFileNames)
            {
                var metadataColumnConfig = GetMetadataColumnConfigDetails(fileName);
                foreach (var metadataColumn in metadataColumnConfig)
                {
                    if (!matchingExtendedConfigData.EdmvColumnName.Contains(metadataColumn.EdmvColumnNameAssocMember))
                    {
                        matchingExtendedConfigData.EdmvColumnName.Add(metadataColumn.EdmvColumnNameAssocMember);
                        matchingExtendedConfigData.EdmvConversion.Add(metadataColumn.EdmvConversionAssocMember);
                        matchingExtendedConfigData.EdmvDatabaseUsageType.Add(metadataColumn.EdmvDatabaseUsageTypeAssocMember);
                        matchingExtendedConfigData.EdmvDateTimeLink.Add(metadataColumn.EdmvDateTimeLinkAssocMember);
                        matchingExtendedConfigData.EdmvFileName.Add(metadataColumn.EdmvFileNameAssocMember);
                        matchingExtendedConfigData.EdmvJsonLabel.Add(metadataColumn.EdmvJsonLabelAssocMember);
                        matchingExtendedConfigData.EdmvJsonPath.Add(metadataColumn.EdmvJsonPathAssocMember);
                        matchingExtendedConfigData.EdmvJsonPropertyType.Add(metadataColumn.EdmvJsonPropertyTypeAssocMember);
                        matchingExtendedConfigData.EdmvLength.Add(metadataColumn.EdmvLengthAssocMember);

                        matchingExtendedConfigData.EdmvColumnsEntityAssociation.Add(metadataColumn);
                    }
                }
            }

            return matchingExtendedConfigData;
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

        private List<EdmExtVersionsEdmvColumns> GetMetadataColumnConfigDetails(string physicalFileName)
        {
            var matchingColumnConfigDetails = new List<EdmExtVersionsEdmvColumns>();

            matchingColumnConfigDetails.Add(new EdmExtVersionsEdmvColumns()
            {
                EdmvColumnNameAssocMember = physicalFileName + ".ADDOPR",
                EdmvDatabaseUsageTypeAssocMember = "D",
                EdmvFileNameAssocMember = physicalFileName,
                EdmvJsonLabelAssocMember = "createdBy",
                EdmvJsonPathAssocMember = "/metadata/",
                EdmvJsonPropertyTypeAssocMember = "string",
                EdmvLengthAssocMember = 10
            });
            matchingColumnConfigDetails.Add(new EdmExtVersionsEdmvColumns()
            {
                EdmvColumnNameAssocMember = physicalFileName + ".ADDDATE",
                EdmvConversionAssocMember = "D2/",
                EdmvDatabaseUsageTypeAssocMember = "D",
                EdmvFileNameAssocMember = physicalFileName,
                EdmvJsonLabelAssocMember = "createdOn",
                EdmvJsonPathAssocMember = "/metadata/",
                EdmvJsonPropertyTypeAssocMember = "datetime",
                EdmvLengthAssocMember = 10,
                EdmvDateTimeLinkAssocMember = "Z"
            });
            matchingColumnConfigDetails.Add(new EdmExtVersionsEdmvColumns()
            {
                EdmvColumnNameAssocMember = physicalFileName + ".ADDTIME",
                EdmvConversionAssocMember = "MTHS",
                EdmvDatabaseUsageTypeAssocMember = "D",
                EdmvFileNameAssocMember = physicalFileName,
                EdmvJsonLabelAssocMember = "createTime",
                EdmvJsonPathAssocMember = "/metadata/",
                EdmvJsonPropertyTypeAssocMember = "time",
                EdmvLengthAssocMember = 10,
                EdmvDateTimeLinkAssocMember = "Z"
            });
            matchingColumnConfigDetails.Add(new EdmExtVersionsEdmvColumns()
            {
                EdmvColumnNameAssocMember = physicalFileName + ".ADD.OPERATOR",
                EdmvDatabaseUsageTypeAssocMember = "D",
                EdmvFileNameAssocMember = physicalFileName,
                EdmvJsonLabelAssocMember = "createdBy",
                EdmvJsonPathAssocMember = "/metadata/",
                EdmvJsonPropertyTypeAssocMember = "string"
            });
            matchingColumnConfigDetails.Add(new EdmExtVersionsEdmvColumns()
            {
                EdmvColumnNameAssocMember = physicalFileName + ".ADD.DATE",
                EdmvConversionAssocMember = "D2/",
                EdmvDatabaseUsageTypeAssocMember = "D",
                EdmvFileNameAssocMember = physicalFileName,
                EdmvJsonLabelAssocMember = "createdOn",
                EdmvJsonPathAssocMember = "/metadata/",
                EdmvJsonPropertyTypeAssocMember = "datetime",
                EdmvLengthAssocMember = 10,
                EdmvDateTimeLinkAssocMember = "Z"
            });
            matchingColumnConfigDetails.Add(new EdmExtVersionsEdmvColumns()
            {
                EdmvColumnNameAssocMember = physicalFileName + ".CHGOPR",
                EdmvDatabaseUsageTypeAssocMember = "D",
                EdmvFileNameAssocMember = physicalFileName,
                EdmvJsonLabelAssocMember = "modifiedBy",
                EdmvJsonPathAssocMember = "/metadata/",
                EdmvJsonPropertyTypeAssocMember = "string",
                EdmvLengthAssocMember = 10
            });
            matchingColumnConfigDetails.Add(new EdmExtVersionsEdmvColumns()
            {
                EdmvColumnNameAssocMember = physicalFileName + ".CHGDATE",
                EdmvConversionAssocMember = "D2/",
                EdmvDatabaseUsageTypeAssocMember = "D",
                EdmvFileNameAssocMember = physicalFileName,
                EdmvJsonLabelAssocMember = "modifiedOn",
                EdmvJsonPathAssocMember = "/metadata/",
                EdmvJsonPropertyTypeAssocMember = "datetime",
                EdmvLengthAssocMember = 10,
                EdmvDateTimeLinkAssocMember = "Y"
            });
            matchingColumnConfigDetails.Add(new EdmExtVersionsEdmvColumns()
            {
                EdmvColumnNameAssocMember = physicalFileName + ".CHGTIME",
                EdmvConversionAssocMember = "MTHS",
                EdmvDatabaseUsageTypeAssocMember = "D",
                EdmvFileNameAssocMember = physicalFileName,
                EdmvJsonLabelAssocMember = "modifiedTime",
                EdmvJsonPathAssocMember = "/metadata/",
                EdmvJsonPropertyTypeAssocMember = "time",
                EdmvLengthAssocMember = 10,
                EdmvDateTimeLinkAssocMember = "Y"
            });
            matchingColumnConfigDetails.Add(new EdmExtVersionsEdmvColumns()
            {
                EdmvColumnNameAssocMember = physicalFileName + ".CHANGE.OPERATOR",
                EdmvDatabaseUsageTypeAssocMember = "D",
                EdmvFileNameAssocMember = physicalFileName,
                EdmvJsonLabelAssocMember = "modifiedBy",
                EdmvJsonPathAssocMember = "/metadata/",
                EdmvJsonPropertyTypeAssocMember = "string",
                EdmvLengthAssocMember = 10
            });
            matchingColumnConfigDetails.Add(new EdmExtVersionsEdmvColumns()
            {
                EdmvColumnNameAssocMember = physicalFileName + ".CHANGE.DATE",
                EdmvConversionAssocMember = "D2/",
                EdmvDatabaseUsageTypeAssocMember = "D",
                EdmvFileNameAssocMember = physicalFileName,
                EdmvJsonLabelAssocMember = "modifiedOn",
                EdmvJsonPathAssocMember = "/metadata/",
                EdmvJsonPropertyTypeAssocMember = "datetime",
                EdmvLengthAssocMember = 10,
                EdmvDateTimeLinkAssocMember = "Y"
            });

            return matchingColumnConfigDetails;
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
                    associationController = dateColumnConfig.EdmvAssociationControllerAssocMember,
                    transType = dateColumnConfig.EdmvTransTypeAssocMember,
                    databaseUsageType = dateColumnConfig.EdmvDatabaseUsageTypeAssocMember

                };

                returnList.Add(returnEthosDataRow);
            }

            return returnList;
        }

        private string ConvertColleagueValueToExtensibleStringValue(EdmExtVersionsEdmvColumns columnConfigDetails, string colleagueValue)
        {
            int dateOrTimeIntFromValue;

            switch (columnConfigDetails.EdmvJsonPropertyTypeAssocMember.ToLower())
            {
                case "string":
                    return colleagueValue;
                case "date":
                    if (int.TryParse(colleagueValue, out dateOrTimeIntFromValue))
                    {
                        var convertedDate = Dmi.Runtime.DmiString.PickDateToDateTime(dateOrTimeIntFromValue);
                        return convertedDate.ToString("yyyy'-'MM'-'dd");
                    }
                    break;
                case "time":
                    if (int.TryParse(colleagueValue, out dateOrTimeIntFromValue))
                    {
                        var convertedDate = Dmi.Runtime.DmiString.PickTimeToDateTime(dateOrTimeIntFromValue);
                        return convertedDate.ToString();
                    }
                    break;
                case "number":
                    //TODO: Replace with call into DMI when correct method is made public.
                    return ConvertColleagueNumber(colleagueValue, columnConfigDetails.EdmvConversionAssocMember);
                default:
                    return colleagueValue;
            }

            return colleagueValue;
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
            catch (Exception e)
            {
                logger.Info(e, "Error retrieving COREWEB.DEFAULTS record");
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
                        throw new Exception(errorMessage);
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
            TaxFormConfiguration2 configuration = new TaxFormConfiguration2(TaxFormTypes.FormT4);
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

        private async Task<TaxFormConfiguration2> GetT4ATaxFormConsentParagraphs2Async()
        {
            TaxFormConfiguration2 configuration = new TaxFormConfiguration2(TaxFormTypes.FormT4A);
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

        private async Task<TaxFormConfiguration2> GetT2202ATaxFormConsentParagraphs2Async()
        {
            TaxFormConfiguration2 configuration = new TaxFormConfiguration2(TaxFormTypes.FormT2202A);
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
        ///                                 CODE HOOKS                                ///
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
                            "", calcProperties.EdmcJsonLabel, calcProperties.EdmcJsonPath, calcProperties.EdmcJsonPropertyType, outputData);

                        extensibleCalculatedRows.Add(returnEthosDataRow);
                    }
                }
            }

            return extensibleCalculatedRows;
        }

        #endregion
    }
}