/*Copyright 2020 Ellucian Company L.P. and its affiliates. */

using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Dmi.Runtime;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Base.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class CollectionConfigurationSettingsRepository : BaseColleagueRepository, ICollectionConfigurationSettingsRepository
    {
        public RepositoryException exception = new RepositoryException();
        public static char _VM = Convert.ToChar(DynamicArray.VM);
        private readonly int _readSize;

        public CollectionConfigurationSettingsRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            CacheTimeout = Level1CacheTimeoutValue;
            this._readSize = ((apiSettings != null) && (apiSettings.BulkReadSize > 0)) ? apiSettings.BulkReadSize : 5000;
        }

        /// <summary>
        /// Get a collection of IntgConfigSettings
        /// </summary>
        /// <param name="bypassCache">Bypass cache flag</param>
        /// <returns>Collection of IntgConfigSettings</returns>
        public async Task<IEnumerable<CollectionConfigurationSettings>> GetCollectionConfigurationSettingsAsync(bool bypassCache)
        {
            if (bypassCache && ContainsKey(BuildFullCacheKey("AllIntgCollectConfigSettings")))
            {
                ClearCache(new List<string> { "AllIntgCollectConfigSettings" });
            }
            return await GetOrAddToCacheAsync<IEnumerable<CollectionConfigurationSettings>>("AllIntgCollectConfigSettings",
                async () =>
                {
                    var configSettingsIds = await DataReader.SelectAsync("INTG.COLLECT.SETTINGS", "");
                    var configSettingsDataContracts = await DataReader.BulkReadRecordAsync<IntgCollectSettings>("INTG.COLLECT.SETTINGS", configSettingsIds);
                    return await BuildCollectionConfigurationSettingsAsync(configSettingsDataContracts, bypassCache);

                }, Level1CacheTimeoutValue);
        }


        /// <summary>
        /// Get a single Configuration Settings domain entity from an academic credential guid.
        /// </summary>
        /// <param name="guid">Guid for Lookup</param>
        /// <param name="bypassCache"> Bypass Cache flag</param>
        /// <returns></returns>
        public async Task<CollectionConfigurationSettings> GetCollectionConfigurationSettingsByGuidAsync(string guid, bool bypassCache = false)
        {
            var configSettings = await GetCollectionConfigurationSettingsAsync(bypassCache);
            if (configSettings == null || !configSettings.Any())
            {
                throw new KeyNotFoundException(string.Format("No configuration-settings resource was found for GUID '{0}'.", guid));
            }
            var configSetting = configSettings.FirstOrDefault(cs => cs.Guid.Equals(guid, StringComparison.InvariantCultureIgnoreCase));
            if (configSetting == null)
            {
                return await GetCollectionConfigurationSettingsByIdAsync(await GetCollectionConfigurationSettingsIdFromGuidAsync(guid), bypassCache);
            }
            return configSetting;
        }


        /// <summary>
        /// Get the record key from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>Primary key</returns>
        public async Task<string> GetCollectionConfigurationSettingsIdFromGuidAsync(string guid)
        {

            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }
            var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(guid) });
            if (idDict == null || idDict.Count == 0)
            {
                throw new KeyNotFoundException("INTG.COLLECT.SETTINGS GUID " + guid + " not found.");
            }
            var foundEntry = idDict.FirstOrDefault();
            if (foundEntry.Value == null || foundEntry.Value.Entity != "INTG.COLLECT.SETTINGS" || !string.IsNullOrEmpty(foundEntry.Value.SecondaryKey))
            {
                throw new KeyNotFoundException(string.Format("The GUID specified: '{0}' is used by a different entity/secondary key: {1}", guid, foundEntry.Value != null ? foundEntry.Value.Entity : string.Empty));
            }
            return foundEntry.Value.PrimaryKey;
        }


        /// <summary>
        /// Get a single Configuration Settings domain entity from an INTG.COLLECT.CONFIG id.
        /// </summary>
        /// <param name="id">The CollectionConfigurationSettings id</param>
        /// <param name="bypassCache">Bypass Cache flag</param>
        /// <returns>Configuration Settings domain entity object</returns>
        private async Task<CollectionConfigurationSettings> GetCollectionConfigurationSettingsByIdAsync(string id, bool bypassCache)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID is required to get a INTG.COLLECT.CONFIG.");
            }

            var configSettings = await DataReader.ReadRecordAsync<IntgCollectSettings>(id);
            if (configSettings == null)
            {
                throw new KeyNotFoundException(string.Concat("Record not found, or INTG.COLLECT.CONFIG with ID ", id, "invalid."));
            }

            var collectionConfigurationSetting = await BuildCollectionConfigurationSettingsAsync(configSettings, bypassCache);

            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }

            return collectionConfigurationSetting;
        }

        #region Update CollectionConfigurationSettings

        public async Task<CollectionConfigurationSettings> UpdateCollectionConfigurationSettingsAsync(CollectionConfigurationSettings collectionConfigurationSettings)
        {
            if (collectionConfigurationSettings == null)
            {
                throw new ArgumentNullException("collectionConfigurationSettingsEntity", "Must provide a collectionConfigurationSettingsEntityEntity to update.");
            }

            if (string.IsNullOrEmpty(collectionConfigurationSettings.Guid))
            {
                throw new ArgumentNullException("collectionConfigurationSettingsEntity", "Must provide the guid of the collectionConfigurationSettingsEntityEntity to update.");
            }

            // verify the GUID exists to perform an update.  If not, perform a create instead
            var collectionConfigurationSettingsId = await GetCollectionConfigurationSettingsIdFromGuidAsync(collectionConfigurationSettings.Guid);

            if (collectionConfigurationSettingsId != null && !string.IsNullOrEmpty(collectionConfigurationSettingsId))
            {
                var updateRequest = BuildUpdateRequest(collectionConfigurationSettings);

                // write the  data
                var updateResponse = await transactionInvoker.ExecuteAsync<UpdateCollectConfigSettingsRequest, UpdateCollectConfigSettingsResponse>(updateRequest);

                if (updateResponse.CollectionConfigErrors.Any())
                {
                    var errorMessage = string.Format("Error(s) occurred updating collection-configuration-settings '{0}':", collectionConfigurationSettings.Guid);
                    var exception = new RepositoryException(errorMessage);
                    updateResponse.CollectionConfigErrors.ForEach(e => exception.AddError(new RepositoryError("Validation.Exception", string.Concat(e.ErrorCodes, ": ", e.ErrorMessages))
                        {
                            Id = collectionConfigurationSettings.Guid,
                            SourceId = collectionConfigurationSettingsId
                        }
                    ));
                    logger.Error(errorMessage);
                    throw exception;
                }
            }
            // get the updated entity from the database
            return await GetCollectionConfigurationSettingsByGuidAsync(collectionConfigurationSettings.Guid, true);
        }

        /// <summary>
        /// Create an ApplicationSupportingItemRequest from a domain entity
        /// </summary>
        /// <param name="collectionConfigurationSettingsEntity">supporting item domain entity</param>
        /// <returns>UpdateApplicationSupportingItemRequest transaction object</returns>
        private UpdateCollectConfigSettingsRequest BuildUpdateRequest(CollectionConfigurationSettings collectionConfigurationSettingsEntity)
        {
            var request = new UpdateCollectConfigSettingsRequest()
            {
                Guid = collectionConfigurationSettingsEntity.Guid,
                ConfigId = collectionConfigurationSettingsEntity.Code,
                ConfigTitle = collectionConfigurationSettingsEntity.Description,
                Description = collectionConfigurationSettingsEntity.FieldHelp
            }; 
            if (collectionConfigurationSettingsEntity.EthosResources != null && collectionConfigurationSettingsEntity.EthosResources.Any())
            {
                foreach (var resource in collectionConfigurationSettingsEntity.EthosResources)
                {
                    request.CollectionResources.Add(new CollectionResources()
                    {
                        Resources = resource.Resource,
                        ResourcePropertyNames = resource.PropertyName
                    });
                }
            }
            if (collectionConfigurationSettingsEntity.Source != null && collectionConfigurationSettingsEntity.Source.Any())
            {
                foreach (var source in collectionConfigurationSettingsEntity.Source)
                {
                    request.CollectionConfigSourceSettings.Add(new CollectionConfigSourceSettings()
                    {
                        SourceTitle = source.SourceTitle,
                        SourceValue = source.SourceValue
                    });
                }
            }

            var extendedDataTuple = GetEthosExtendedDataLists();
            if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
            {
                request.ExtendedNames = extendedDataTuple.Item1;
                request.ExtendedValues = extendedDataTuple.Item2;
            }

            return request;
        }

        #endregion

        #region Build CollectionConfigurationSettings

        /// <summary>
        /// Build from a Collection of Data Contracts
        /// </summary>
        /// <param name="sources"></param>
        /// <returns></returns>
        private async Task<IEnumerable<CollectionConfigurationSettings>> BuildCollectionConfigurationSettingsAsync(Collection<IntgCollectSettings> sources, bool bypassCache = false)
        {
            var configSettingsCollection = new List<CollectionConfigurationSettings>();

            foreach (var source in sources)
            {
                configSettingsCollection.Add(await BuildCollectionConfigurationSettingsAsync(source, bypassCache));
            }

            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }

            return configSettingsCollection.AsEnumerable();
        }

        /// <summary>
        /// Build from a single data contract
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private async Task<CollectionConfigurationSettings> BuildCollectionConfigurationSettingsAsync(IntgCollectSettings source, bool bypassCache = false)
        {
            if (source == null)
            {
                exception.AddError(new RepositoryError("Bad.Data", "Selected, but can not read INTG.CONFIG.SETTINGS record."));
                return null;
            }

            CollectionConfigurationSettings collectionConfigurationSetting = null;

            try
            {
                collectionConfigurationSetting = new CollectionConfigurationSettings(source.RecordGuid, source.Recordkey, source.IclCollDefaultTitle)
                {
                    EthosResources = new List<DefaultSettingsResource>(),
                    FieldHelp = source.IclCollDefaultDesc,
                    EntityName = source.IclCollEntity,
                    FieldName = source.IclCollLdmFieldName,
                    ValcodeTableName = source.IclCollValcodeId
                };
                if (source.CollectResourcesEntityAssociation != null && source.CollectResourcesEntityAssociation.Any())
                {
                    foreach (var resource in source.CollectResourcesEntityAssociation)
                    {
                        if (!string.IsNullOrEmpty(resource.IclEthosResourcesAssocMember))
                        {
                            collectionConfigurationSetting.EthosResources.Add(new DefaultSettingsResource()
                            {
                                Resource = resource.IclEthosResourcesAssocMember,
                                PropertyName = resource.IclEthosPropertyNamesAssocMember
                            });
                        }
                    }
                }
                else
                {
                    exception.AddError(new RepositoryError("Missing.Required.Property", "Invalid INTG.COLLECT.SETTINGS record.  Missing ethos.resource property.")
                    {
                        Id = source.RecordGuid,
                        SourceId = source.Recordkey
                    });
                }
                collectionConfigurationSetting = await BuildSourceTitleAndValue(collectionConfigurationSetting, source, bypassCache);
            }
            catch (Exception ex)
            {
                exception.AddError(new RepositoryError("Missing.Required.Property", "Invalid INTG.COLLECT.SETTINGS record.  Missing one or more required property.  " + ex.Message)
                {
                    Id = source.RecordGuid,
                    SourceId = source.Recordkey
                });
            }

            return collectionConfigurationSetting;
        }

        private async Task<CollectionConfigurationSettings> BuildSourceTitleAndValue(CollectionConfigurationSettings collectionConfigurationSetting, IntgCollectSettings configSettingsDataContract, bool bypassCache = false)
        {
            if (collectionConfigurationSetting == null)
            {
                throw new KeyNotFoundException("Could not find a CollectionConfigurationSettings record.");
            }

            var ldmDefaults = await GetLdmDefaults(bypassCache);
            var collectionConfigSource = new List<CollectionConfigurationSettingsSource>();

            switch (configSettingsDataContract.IclCollLdmFieldName.ToUpperInvariant())
            {
                case "LDMD.INCLUDE.ENRL.HEADCOUNTS":
                    {
                        foreach (var includeCode in ldmDefaults.LdmdIncludeEnrlHeadcounts)
                        {
                            var source = new CollectionConfigurationSettingsSource()
                            {
                                SourceValue = includeCode,
                                SourceTitle = await GetStudentAcadCredStatusAsync(includeCode, bypassCache)
                            };
                            collectionConfigSource.Add(source);
                        }
                        break;
                    }
                case "LDMD.DFLT.ADM.OFFICE.CODES":
                    {
                        foreach (var includeCode in ldmDefaults.LdmdDfltAdmOfficeCodes)
                        {
                            var source = new CollectionConfigurationSettingsSource()
                            {
                                SourceValue = includeCode,
                                SourceTitle = await GetOfficeCodeAsync(includeCode, bypassCache)
                            };
                            collectionConfigSource.Add(source);
                        }
                        break;
                    }
                case "LDMD.EXCLUDE.BENEFITS":
                    {
                        foreach(var includeCode in ldmDefaults.LdmdExcludeBenefits)
                        {
                            var source = new CollectionConfigurationSettingsSource()
                            {
                                SourceValue = includeCode,
                                SourceTitle = await GetBendedCodeAsync(includeCode, bypassCache)
                            };
                            collectionConfigSource.Add(source);
                        }
                        break;
                    }
                case "LDMD.LEAVE.STATUS.CODES":
                    {
                        foreach (var includeCode in ldmDefaults.LdmdLeaveStatusCodes)
                        {
                            var source = new CollectionConfigurationSettingsSource()
                            {
                                SourceValue = includeCode,
                                SourceTitle = await GetLeaveStatusAsync(includeCode, bypassCache)
                            };
                            collectionConfigSource.Add(source);
                        }
                        break;
                    }
                case "LDMD.GUARDIAN.REL.TYPES":
                    {
                        foreach (var includeCode in ldmDefaults.LdmdGuardianRelTypes)
                        {
                            var source = new CollectionConfigurationSettingsSource()
                            {
                                SourceValue = includeCode,
                                SourceTitle = await GetRelationTypesCodeAsync(includeCode, bypassCache)
                            };
                            collectionConfigSource.Add(source);
                        }
                        break;
                    }
            }
            collectionConfigurationSetting.Source = collectionConfigSource;

            return collectionConfigurationSetting;
        }
        #endregion

        #region Helper Methods (Public)

        /// <summary>
        /// Retrieve all BENDED records from cache and return dictionary
        /// of key and description.
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns>"Dictionary of key value pair, key and description.</returns>
        public async Task<Dictionary<string, string>> GetAllBendedCodesAsync(bool bypassCache)
        {
            Dictionary<string, string> dictBendedCodes = new Dictionary<string, string>();

            try
            {
                var cacheControlKey = "AllBendedCodesCollectConfigSettings";
                if (bypassCache && ContainsKey(BuildFullCacheKey(cacheControlKey)))
                {
                    ClearCache(new List<string> { cacheControlKey });
                }
                var allBendedCodes = await GetOrAddToCacheAsync<IEnumerable<BendedBase>>(cacheControlKey,
                    async () =>
                    {
                        var BendedCodes = await DataReader.BulkReadRecordAsync<BendedBase>("BENDED", "");
                        if (BendedCodes == null)
                        {
                            logger.Info("Unable to access BENDED from database.");
                            BendedCodes = new Collection<BendedBase>();
                        }
                        return BendedCodes;
                    }, Level1CacheTimeoutValue);

                if (allBendedCodes != null && allBendedCodes.Any())
                {
                    foreach (var bendedCode in allBendedCodes)
                    {
                        if (bendedCode != null && !string.IsNullOrEmpty(bendedCode.BdDesc))
                        {
                            var dictKey = bendedCode.Recordkey;
                            var dictValue = bendedCode.BdDesc;
                            if (!string.IsNullOrEmpty(dictKey) && !dictBendedCodes.ContainsKey(dictKey))
                            {
                                dictBendedCodes.Add(dictKey, dictValue);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            { 
                // do nothing.. 
            }

            return dictBendedCodes;
        }

        /// <summary>
        /// Retrieve all RELATION.TYPES records for guardians from cache and return dictionary
        /// of key and description.
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns>"Dictionary of key value pair, key and description.</returns>
        public async Task<Dictionary<string, string>> GetAllRelationTypesCodesAsync(bool bypassCache)
        {
            Dictionary<string, string> dictBendedCodes = new Dictionary<string, string>();

            var cacheControlKey = "AllRelationTypesCodesCollectConfigSettings";
            if (bypassCache && ContainsKey(BuildFullCacheKey(cacheControlKey)))
            {
                ClearCache(new List<string> { cacheControlKey });
            }
            var allRelationTypesCodes = await GetOrAddToCacheAsync<IEnumerable<RelationTypes>>(cacheControlKey,
                async () =>
                {
                    var relationTypesCodes = await DataReader.BulkReadRecordAsync<RelationTypes>("RELATION.TYPES", "WITH RELTY.ORG.INDICATOR NE 'Y'");
                    if (relationTypesCodes == null)
                    {
                        logger.Info("Unable to access RELATION.TYPES from database.");
                        relationTypesCodes = new Collection<RelationTypes>();
                    }
                    return relationTypesCodes;
                }, Level1CacheTimeoutValue);

            if (allRelationTypesCodes != null && allRelationTypesCodes.Any())
            {
                foreach (var bendedCode in allRelationTypesCodes)
                {
                    if (bendedCode != null && !string.IsNullOrEmpty(bendedCode.ReltyDesc))
                    {
                        var dictKey = bendedCode.Recordkey;
                        var dictValue = bendedCode.ReltyDesc;
                        if (!string.IsNullOrEmpty(dictKey) && !dictBendedCodes.ContainsKey(dictKey))
                        {
                            dictBendedCodes.Add(dictKey, dictValue);
                        }
                    }
                }
            }

            return dictBendedCodes;
        }

        public async Task<Dictionary<string, string>> GetAllValcodeItemsAsync(string entity, string valcodeTable, bool bypassCache)
        {
            Dictionary<string, string> dictValcodeItems = new Dictionary<string, string>();

            var cacheControlKey = string.Concat("AllValcodesCollectConfigSettings", entity, valcodeTable);
            if (bypassCache && ContainsKey(BuildFullCacheKey(cacheControlKey)))
            {
                ClearCache(new List<string> { cacheControlKey });
            }
            return await GetOrAddToCacheAsync<Dictionary<string, string>>(cacheControlKey,
                async () =>
                {
                    var valcode = await DataReader.ReadRecordAsync<Ellucian.Data.Colleague.DataContracts.ApplValcodes>(entity, valcodeTable);

                    if (valcode != null)
                    {
                        foreach (var row in valcode.ValsEntityAssociation)
                        {
                            var dictKey = row.ValInternalCodeAssocMember;
                            var dictValue = row.ValExternalRepresentationAssocMember;
                            if (!string.IsNullOrEmpty(dictKey) && !string.IsNullOrEmpty(dictValue) && !dictValcodeItems.ContainsKey(dictKey))
                            {
                                dictValcodeItems.Add(dictKey, dictValue);
                            }
                        }
                    }
                    return dictValcodeItems;
                }, Level1CacheTimeoutValue);
        }
        #endregion

        #region Helper Methods (Private)
        /// <summary>
        /// Get the LdmDefaults from CORE
        /// </summary>
        /// <returns>Core Defaults</returns>
        private async Task<Base.DataContracts.LdmDefaults> GetLdmDefaults(bool bypassCache = false)
        {
            var cacheControlKey = "LdmDefaultsCollectConfigSettings";
            if (bypassCache && ContainsKey(BuildFullCacheKey(cacheControlKey)))
            {
                ClearCache(new List<string> { cacheControlKey });
            }
            return await GetOrAddToCacheAsync<Data.Base.DataContracts.LdmDefaults>(cacheControlKey,
                async () =>
                {
                    var coreDefaults = await DataReader.ReadRecordAsync<Data.Base.DataContracts.LdmDefaults>("CORE.PARMS", "LDM.DEFAULTS");
                    if (coreDefaults == null)
                    {
                        logger.Info("Unable to access DEFAULTS from CORE.PARMS table.");
                        coreDefaults = new LdmDefaults();
                    }
                    return coreDefaults;
                }, Level1CacheTimeoutValue);
        }

        private async Task<string> GetStudentAcadCredStatusAsync(string sourceValue, bool bypassCache)
        {
            if (string.IsNullOrEmpty(sourceValue))
            {
                return string.Empty;
            }

            var dictValcodes = await GetAllValcodeItemsAsync("ST.VALCODES", "STUDENT.ACAD.CRED.STATUSES", bypassCache);

            if (!dictValcodes.ContainsKey(sourceValue))
            {
                throw new KeyNotFoundException(string.Format("Could not find a CollectionConfigurationSettings source title for value: '{0}'.", sourceValue));
            }
            else
            {
                string sourceTitle = string.Empty;
                dictValcodes.TryGetValue(sourceValue, out sourceTitle);
                if (string.IsNullOrEmpty(sourceTitle))
                {
                    throw new KeyNotFoundException(string.Format("Could not find a CollectionConfigurationSettings source title for value: '{0}'.", sourceValue));
                }
                else
                {
                    return sourceTitle;
                }
            }
        }

        private async Task<string> GetOfficeCodeAsync(string sourceValue, bool bypassCache)
        {
            if (string.IsNullOrEmpty(sourceValue))
            {
                return string.Empty;
            }

            var dictValcodes = await GetAllValcodeItemsAsync("CORE.VALCODES", "OFFICE.CODES", bypassCache);

            if (!dictValcodes.ContainsKey(sourceValue))
            {
                throw new KeyNotFoundException(string.Format("Could not find a CollectionConfigurationSettings source title for value: '{0}'.", sourceValue));
            }
            else
            {
                string sourceTitle = string.Empty;
                dictValcodes.TryGetValue(sourceValue, out sourceTitle);
                if (string.IsNullOrEmpty(sourceTitle))
                {
                    throw new KeyNotFoundException(string.Format("Could not find a CollectionConfigurationSettings source title for value: '{0}'.", sourceValue));
                }
                else
                {
                    return sourceTitle;
                }
            }
        }

        private async Task<string> GetBendedCodeAsync(string sourceValue, bool bypassCache)
        {
            if (string.IsNullOrEmpty(sourceValue))
            {
                return string.Empty;
            }

            if (bypassCache)
            {
                var bended = await DataReader.ReadRecordAsync<BendedBase>(sourceValue);
                if (bended == null || string.IsNullOrEmpty(bended.BdDesc))
                {
                    throw new KeyNotFoundException(string.Format("Could not find a DefaultSettings source title for value: '{0}'.", sourceValue));
                }
                return bended.BdDesc;
            }
            var dictBendedCodes = await GetAllBendedCodesAsync(bypassCache);

            if (!dictBendedCodes.ContainsKey(sourceValue))
            {
                throw new KeyNotFoundException(string.Format("Could not find a CollectionConfigurationSettings source title for value: '{0}'.", sourceValue));
            }
            else
            {
                string sourceTitle = string.Empty;
                dictBendedCodes.TryGetValue(sourceValue, out sourceTitle);
                if (string.IsNullOrEmpty(sourceTitle))
                {
                    throw new KeyNotFoundException(string.Format("Could not find a CollectionConfigurationSettings source title for value: '{0}'.", sourceValue));
                }
                else
                {
                    return sourceTitle;
                }
            }
        }

        private async Task<string> GetLeaveStatusAsync(string sourceValue, bool bypassCache)
        {
            if (string.IsNullOrEmpty(sourceValue))
            {
                return string.Empty;
            }

            var dictValcodes = await GetAllValcodeItemsAsync("HR.VALCODES", "HR.STATUSES", bypassCache);

            if (!dictValcodes.ContainsKey(sourceValue))
            {
                throw new KeyNotFoundException(string.Format("Could not find a CollectionConfigurationSettings source title for value: '{0}'.", sourceValue));
            }
            else
            {
                string sourceTitle = string.Empty;
                dictValcodes.TryGetValue(sourceValue, out sourceTitle);
                if (string.IsNullOrEmpty(sourceTitle))
                {
                    throw new KeyNotFoundException(string.Format("Could not find a CollectionConfigurationSettings source title for value: '{0}'.", sourceValue));
                }
                else
                {
                    return sourceTitle;
                }
            }
        }

        private async Task<string> GetRelationTypesCodeAsync(string sourceValue, bool bypassCache)
        {
            if (string.IsNullOrEmpty(sourceValue))
            {
                return string.Empty;
            }

            if (bypassCache)
            {
                var bended = await DataReader.ReadRecordAsync<RelationTypes>(sourceValue);
                if (bended == null || string.IsNullOrEmpty(bended.ReltyDesc))
                {
                    throw new KeyNotFoundException(string.Format("Could not find a DefaultSettings source title for value: '{0}'.", sourceValue));
                }
                return bended.ReltyDesc;
            }
            var dictBendedCodes = await GetAllRelationTypesCodesAsync(bypassCache);

            if (!dictBendedCodes.ContainsKey(sourceValue))
            {
                throw new KeyNotFoundException(string.Format("Could not find a CollectionConfigurationSettings source title for value: '{0}'.", sourceValue));
            }
            else
            {
                string sourceTitle = string.Empty;
                dictBendedCodes.TryGetValue(sourceValue, out sourceTitle);
                if (string.IsNullOrEmpty(sourceTitle))
                {
                    throw new KeyNotFoundException(string.Format("Could not find a CollectionConfigurationSettings source title for value: '{0}'.", sourceValue));
                }
                else
                {
                    return sourceTitle;
                }
            }
        }

        #endregion
    }
}