/*Copyright 2019-2020 Ellucian Company L.P. and its affiliates. */

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
    public class ConfigurationSettingsRepository : BaseColleagueRepository, IConfigurationSettingsRepository
    {
        public RepositoryException exception = new RepositoryException();
        public static char _VM = Convert.ToChar(DynamicArray.VM);
        private readonly int _readSize;

        public ConfigurationSettingsRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
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
        public async Task<IEnumerable<ConfigurationSettings>> GetConfigurationSettingsAsync(bool bypassCache)
        {
            if (bypassCache && ContainsKey(BuildFullCacheKey("AllIntgConfigSettings")))
            {
                ClearCache(new List<string> { "AllIntgConfigSettings" });
            }
            return await GetOrAddToCacheAsync<IEnumerable<ConfigurationSettings>>("AllIntgConfigSettings",
                async () =>
                {
                    var configSettingsIds = await DataReader.SelectAsync("INTG.CONFIG.SETTINGS", "");
                    var configSettingsDataContracts = await DataReader.BulkReadRecordAsync<IntgConfigSettings>("INTG.CONFIG.SETTINGS", configSettingsIds);
                    return await BuildConfigurationSettingsAsync(configSettingsDataContracts, bypassCache);

                }, Level1CacheTimeoutValue);
        }


        /// <summary>
        /// Get a single Configuration Settings domain entity from an academic credential guid.
        /// </summary>
        /// <param name="guid">Guid for Lookup</param>
        /// <param name="bypassCache"> Bypass Cache flag</param>
        /// <returns></returns>
        public async Task<ConfigurationSettings> GetConfigurationSettingsByGuidAsync(string guid, bool bypassCache = false)
        {
            var configSettings = await GetConfigurationSettingsAsync(bypassCache);
            if (configSettings == null || !configSettings.Any())
            {
                throw new KeyNotFoundException(string.Format("No configuration-settings resource was found for GUID '{0}'.", guid));
            }
            var configSetting = configSettings.FirstOrDefault(cs => cs.Guid.Equals(guid, StringComparison.InvariantCultureIgnoreCase));
            if (configSetting == null)
            {
                return await GetConfigurationSettingsByIdAsync(await GetConfigurationSettingsIdFromGuidAsync(guid), bypassCache);
            }
            return configSetting;
        }


        /// <summary>
        /// Get the record key from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>Primary key</returns>
        public async Task<string> GetConfigurationSettingsIdFromGuidAsync(string guid)
        {

            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }
            var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(guid) });
            if (idDict == null || idDict.Count == 0)
            {
                throw new KeyNotFoundException("INTG.CONFIG.SETTINGS GUID " + guid + " not found.");
            }
            var foundEntry = idDict.FirstOrDefault();
            if (foundEntry.Value == null || foundEntry.Value.Entity != "INTG.CONFIG.SETTINGS" || !string.IsNullOrEmpty(foundEntry.Value.SecondaryKey))
            {
                throw new KeyNotFoundException(string.Format("The GUID specified: '{0}' is used by a different entity/secondary key: {1}", guid, foundEntry.Value != null ? foundEntry.Value.Entity : string.Empty));
            }
            return foundEntry.Value.PrimaryKey;
        }


        /// <summary>
        /// Get a single Configuration Settings domain entity from an academic credential id.
        /// </summary>
        /// <param name="id">The ConfigurationSettings id</param>
        /// <param name="bypassCache">Bypass Cache flag</param>
        /// <returns>Configuration Settings domain entity object</returns>
        private async Task<ConfigurationSettings> GetConfigurationSettingsByIdAsync(string id, bool bypassCache)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID is required to get a AcademicCredentials.");
            }

            var configSettings = await DataReader.ReadRecordAsync<IntgConfigSettings>(id);
            if (configSettings == null)
            {
                throw new KeyNotFoundException(string.Concat("Record not found, or AcademicCredentials with ID ", id, "invalid."));
            }

            var configurationSetting = await BuildConfigurationSettingsAsync(configSettings, bypassCache);

            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }

            return configurationSetting;
        }

        #region Update ConfigurationSettings

        public async Task<ConfigurationSettings> UpdateConfigurationSettingsAsync(ConfigurationSettings configurationSettings)
        {
            if (configurationSettings == null)
            {
                throw new ArgumentNullException("configurationSettingsEntity", "Must provide a configurationSettingsEntityEntity to update.");
            }

            if (string.IsNullOrEmpty(configurationSettings.Guid))
            {
                throw new ArgumentNullException("configurationSettingsEntity", "Must provide the guid of the configurationSettingsEntityEntity to update.");
            }

            // verify the GUID exists to perform an update.  If not, perform a create instead
            var configurationSettingsId = await GetConfigurationSettingsIdFromGuidAsync(configurationSettings.Guid);

            if (configurationSettingsId != null && !string.IsNullOrEmpty(configurationSettingsId))
            {
                var updateRequest = BuildUpdateRequest(configurationSettings);

                // write the  data
                var updateResponse = await transactionInvoker.ExecuteAsync<UpdateConfigurationSettingsRequest, UpdateConfigurationSettingsResponse>(updateRequest);

                if (updateResponse.ConfigSettingErrors.Any())
                {
                    var errorMessage = string.Format("Error(s) occurred updating configuration-settings '{0}':", configurationSettings.Guid);
                    var exception = new RepositoryException(errorMessage);
                    updateResponse.ConfigSettingErrors.ForEach(e => exception.AddError(new RepositoryError("Validation.Exception", string.Concat(e.ErrorCodes, ": ", e.ErrorMessages))
                        {
                            Id = configurationSettings.Guid,
                            SourceId = configurationSettingsId
                        }
                    ));
                    logger.Error(errorMessage);
                    throw exception;
                }
            }
            // get the updated entity from the database
            return await GetConfigurationSettingsByGuidAsync(configurationSettings.Guid, true);
        }

        /// <summary>
        /// Create an ApplicationSupportingItemRequest from a domain entity
        /// </summary>
        /// <param name="configurationSettingsEntity">supporting item domain entity</param>
        /// <returns>UpdateApplicationSupportingItemRequest transaction object</returns>
        private UpdateConfigurationSettingsRequest BuildUpdateRequest(ConfigurationSettings configurationSettingsEntity)
        {
            var request = new UpdateConfigurationSettingsRequest()
            {
                Guid = configurationSettingsEntity.Guid,
                ConfigId = configurationSettingsEntity.Code,
                ConfigTitle = configurationSettingsEntity.Description,
                Resources = configurationSettingsEntity.EthosResources,
                Description = configurationSettingsEntity.FieldHelp,
                SourceTitle = configurationSettingsEntity.SourceTitle,
                SourceValue = configurationSettingsEntity.SourceValue
            };

            var extendedDataTuple = GetEthosExtendedDataLists();
            if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
            {
                request.ExtendedNames = extendedDataTuple.Item1;
                request.ExtendedValues = extendedDataTuple.Item2;
            }

            return request;
        }

        #endregion

        #region Build ConfigurationSettings

        /// <summary>
        /// Build from a Collection of Data Contracts
        /// </summary>
        /// <param name="sources"></param>
        /// <returns></returns>
        private async Task<IEnumerable<ConfigurationSettings>> BuildConfigurationSettingsAsync(Collection<IntgConfigSettings> sources, bool bypassCache = false)
        {
            var configSettingsCollection = new List<ConfigurationSettings>();

            foreach (var source in sources)
            {
                configSettingsCollection.Add(await BuildConfigurationSettingsAsync(source, bypassCache));
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
        private async Task<ConfigurationSettings> BuildConfigurationSettingsAsync(IntgConfigSettings source, bool bypassCache = false)
        {
            if (source == null)
            {
                exception.AddError(new RepositoryError("Bad.Data", "Selected, but can not read INTG.CONFIG.SETTINGS record."));
                return null;
            }

            ConfigurationSettings configurationSetting = null;

            try
            {
                configurationSetting = new ConfigurationSettings(source.RecordGuid, source.Recordkey, source.IcsCollConfigTitle)
                {
                    EthosResources = source.IcsEthosResources,
                    MultipleValues = !string.IsNullOrEmpty(source.IcsMultipleValue) && source.IcsMultipleValue.Equals("Y", StringComparison.OrdinalIgnoreCase) ? true : false,
                    FieldHelp = source.IcsCollConfigDesc,
                    EntityName = source.IcsCollEntity,
                    FieldName = source.IcsCollLdmFieldName,
                    ValcodeTableName = source.IcsCollValcodeId
                };
                configurationSetting = await BuildSourceTitleAndValue(configurationSetting, source, bypassCache);
            }
            catch (Exception ex)
            {
                exception.AddError(new RepositoryError("Missing.Required.Property", "Invalid INTG.CONFIG.SETTINGS record.  Missing one or more required property.  " + ex.Message)
                {
                    Id = source.RecordGuid,
                    SourceId = source.Recordkey
                });
            }

            return configurationSetting;
        }

        private async Task<ConfigurationSettings> BuildSourceTitleAndValue(ConfigurationSettings configurationSetting, IntgConfigSettings configSettingsDataContract, bool bypassCache = false)
        {
            if (configurationSetting == null)
            {
                throw new KeyNotFoundException("Could not find a ConfigurationSettings record.");
            }

            var ldmDefaults = await GetLdmDefaults(bypassCache);
            var ldmFieldName = configSettingsDataContract.IcsCollLdmFieldName;

            switch (configSettingsDataContract.IcsCollLdmFieldName.ToUpperInvariant())
            {
                case "LDMD.PERSON.DUPL.CRITERIA":
                    {
                        configurationSetting.SourceValue = ldmDefaults.LdmdPersonDuplCriteria;
                        configurationSetting.SourceTitle = await GetDuplicateCriteriaAsync(configurationSetting.SourceValue, bypassCache);
                        break;
                    }
                case "LDMD.ADDR.DUPL.CRITERIA":
                    {
                        configurationSetting.SourceValue = ldmDefaults.LdmdAddrDuplCriteria;
                        configurationSetting.SourceTitle = await GetDuplicateCriteriaAsync(configurationSetting.SourceValue, bypassCache);
                        break;
                    }
                case "LDMD.PROSPECT.DUPL.CRITERIA":
                    {
                        configurationSetting.SourceValue = ldmDefaults.LdmdProspectDuplCriteria;
                        configurationSetting.SourceTitle = await GetDuplicateCriteriaAsync(configurationSetting.SourceValue, bypassCache);
                        break;
                    }
                case "LDMD.CHECK.FACULTY.LOAD":
                    {
                        if (!string.IsNullOrEmpty(ldmDefaults.LdmdCheckFacultyLoad))
                        {
                            configurationSetting.SourceValue = ldmDefaults.LdmdCheckFacultyLoad;
                            configurationSetting.SourceTitle = ldmDefaults.LdmdCheckFacultyLoad.Equals("Y", StringComparison.OrdinalIgnoreCase) ? "Yes" : "No";
                        }
                        break;
                    }
                case "LDMD.REG.USERS.ID":
                    {
                        configurationSetting.SourceValue = ldmDefaults.LdmdRegUsersId;
                        configurationSetting.SourceTitle = ldmDefaults.LdmdRegUsersId;
                        break;
                    }
                case "LDMD.CASHIER":
                    {
                        configurationSetting.SourceValue = ldmDefaults.LdmdCashier;
                        configurationSetting.SourceTitle = await GetCashierNameAsync(configurationSetting.SourceValue, bypassCache);
                        break;
                    }
                case "LDMD.CHECK.POSTING.PERFORMED":
                    {
                        if (!string.IsNullOrEmpty(ldmDefaults.LdmdCheckPostingPerformed))
                        {
                            configurationSetting.SourceValue = ldmDefaults.LdmdCheckPostingPerformed;
                            configurationSetting.SourceTitle = ldmDefaults.LdmdCheckPostingPerformed.Equals("Y", StringComparison.OrdinalIgnoreCase) ? "Yes" : "No";
                        }
                        break;
                    }
                case "LDMD.PRIN.INVESTIGATOR.ROLE":
                    {
                        configurationSetting.SourceValue = ldmDefaults.LdmdPrinInvestigatorRole;
                        configurationSetting.SourceTitle = await GetPrinInvestigatorRoleAsync(configurationSetting.SourceValue, bypassCache);
                        break;
                    }
                case "LDMD.MAPPING.CONTROL":
                    {
                        configurationSetting.SourceValue = "ethos";
                        configurationSetting.SourceTitle = "Update ethos value";
                        break;
                    }
                case "LDMD.RELATION.DUPL.CRITERIA":
                    {
                        configurationSetting.SourceValue = ldmDefaults.LdmdRelationDuplCriteria;
                        configurationSetting.SourceTitle = await GetDuplicateCriteriaAsync(configurationSetting.SourceValue, bypassCache);
                        break;
                    }
                case "LDMD.VENCONTACT.DUP.CRITERIA":
                    {
                        configurationSetting.SourceValue = ldmDefaults.LdmdVencontactDupCriteria;
                        configurationSetting.SourceTitle = await GetDuplicateCriteriaAsync(configurationSetting.SourceValue, bypassCache);
                        break;
                    }
                case "LDMD.ALLOW.MOVE.TO.STU":
                    {
                        if (!string.IsNullOrEmpty(ldmDefaults.LdmdAllowMoveToStu))
                        {
                            configurationSetting.SourceValue = ldmDefaults.LdmdAllowMoveToStu;
                            configurationSetting.SourceTitle = ldmDefaults.LdmdAllowMoveToStu.Equals("Y", StringComparison.OrdinalIgnoreCase) ? "Yes" : "No";
                        }
                        break;
                    }
            }

            return configurationSetting;
        }
        #endregion

        #region Helper Methods (Public)

        /// <summary>
        /// Retrieve all ELF.DUPL.CRITERIA records from cache and return dictionary
        /// of key and description.
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns>"Dictionary of key value pair, key and description.</returns>
        public async Task<Dictionary<string, string>> GetAllDuplicateCriteriaAsync(bool bypassCache)
        {
            Dictionary<string, string> dictDuplicateCriteria = new Dictionary<string, string>();

            var cacheControlKey = "AllDuplicateCriteriaConfigSettings";
            if (bypassCache && ContainsKey(BuildFullCacheKey(cacheControlKey)))
            {
                ClearCache(new List<string> { cacheControlKey });
            }
            var allDuplicateCriteria = await GetOrAddToCacheAsync<IEnumerable<ElfDuplCriteria>>(cacheControlKey,
                async () =>
                {
                    var elfDupls = await DataReader.BulkReadRecordAsync<ElfDuplCriteria>("ELF.DUPL.CRITERIA", "");
                    if (elfDupls == null)
                    {
                        logger.Info("Unable to access ELF.DUPL.CRITERIA from database.");
                        elfDupls = new Collection<ElfDuplCriteria>();
                    }
                    return elfDupls;
                }, Level1CacheTimeoutValue);

            if (allDuplicateCriteria != null && allDuplicateCriteria.Any())
            {
               foreach (var elfDuplCriteria in allDuplicateCriteria)
                {
                    var dictKey = elfDuplCriteria.Recordkey;
                    var dictValue = elfDuplCriteria.ElfduplDesc;
                    if (!string.IsNullOrEmpty(dictKey) && !string.IsNullOrEmpty(dictValue) && !dictDuplicateCriteria.ContainsKey(dictKey))
                    {
                        dictDuplicateCriteria.Add(dictKey, dictValue);
                    }
                }
            }

            return dictDuplicateCriteria;
        }

        public async Task<Dictionary<string, string>> GetAllCashierNamesAsync(bool bypassCache)
        {
            Dictionary<string, string> dictCashierNames = new Dictionary<string, string>();

            var cacheControlKey = "AllStaffNamesConfigSettings";
            if (bypassCache && ContainsKey(BuildFullCacheKey(cacheControlKey)))
            {
                ClearCache(new List<string> { cacheControlKey });
            }
            return await GetOrAddToCacheAsync<Dictionary<string, string>>(cacheControlKey,
                async () =>
                {
                    var cashierKeys = await DataReader.SelectAsync("CASHIERS", "WITH CSHR.ECOMMERCE.FLAG NE 'Y'");
                    if (cashierKeys != null)
                    {
                        var personContracts = await DataReader.BulkReadRecordAsync<DataContracts.Person>("PERSON", cashierKeys);
                        foreach (var person in personContracts)
                        {
                            if (person != null && !string.IsNullOrEmpty(person.LastName))
                            {
                                    var preferred = string.Concat(person.FirstName, " ", person.LastName);
                                    if (!dictCashierNames.ContainsKey(person.Recordkey))
                                    {
                                        dictCashierNames.Add(person.Recordkey, preferred);
                                    }
                            }
                        }
                    }
                    return dictCashierNames;
                }, Level1CacheTimeoutValue);
        }

        public async Task<Dictionary<string, string>> GetAllRegUsersAsync(bool bypassCache)
        {
            Dictionary<string, string> dictRegUsers = new Dictionary<string, string>();

            var cacheControlKey = "AllRegUsersConfigSettings";
            if (bypassCache && ContainsKey(BuildFullCacheKey(cacheControlKey)))
            {
                ClearCache(new List<string> { cacheControlKey });
            }
            return await GetOrAddToCacheAsync<Dictionary<string, string>>(cacheControlKey,
                async () =>
                {
                    var regUserKeys = await DataReader.SelectAsync("REG.USERS", "");

                    foreach (var regUser in regUserKeys)
                    {
                        if (regUser != null && !string.IsNullOrEmpty(regUser))
                        {   
                            if (!dictRegUsers.ContainsKey(regUser))
                            {
                                dictRegUsers.Add(regUser, regUser);
                            }
                        }
                    }
                    return dictRegUsers;
                }, Level1CacheTimeoutValue);
        }

        public async Task<Dictionary<string, string>> GetAllValcodeItemsAsync(string entity, string valcodeTable, bool bypassCache)
        {
            Dictionary<string, string> dictValcodeItems = new Dictionary<string, string>();

            var cacheControlKey = string.Concat("AllRegUsersConfigSettings", entity, valcodeTable);
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
            var cacheControlKey = "LdmDefaultsConfigSettings";
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

        private async Task<string> GetDuplicateCriteriaAsync(string sourceValue, bool bypassCache)
        {
            if (string.IsNullOrEmpty(sourceValue))
            {
                return string.Empty;
            }

            var dictElfDuplCriteria = await GetAllDuplicateCriteriaAsync(bypassCache);

            if (!dictElfDuplCriteria.ContainsKey(sourceValue))
            {
                throw new KeyNotFoundException(string.Format("Could not find a ConfigurationSettings source title for value: '{0}'.", sourceValue));
            }
            else
            {
                string sourceTitle = string.Empty;
                dictElfDuplCriteria.TryGetValue(sourceValue, out sourceTitle);
                if (string.IsNullOrEmpty(sourceTitle))
                {
                    throw new KeyNotFoundException(string.Format("Could not find a ConfigurationSettings source title for value: '{0}'.", sourceValue));
                }
                else
                {
                    return sourceTitle;
                }
            }
        }

        private async Task<string> GetCashierNameAsync(string sourceValue, bool bypassCache)
        {
            if (string.IsNullOrEmpty(sourceValue))
            {
                return string.Empty;
            }
            var person = await DataReader.ReadRecordAsync<DataContracts.Person>("PERSON", sourceValue);
            if (person == null || string.IsNullOrEmpty(person.LastName))
            {
                throw new KeyNotFoundException(string.Format("Could not find a ConfigurationSettings source title for value: '{0}'.", sourceValue));
            }
            else
            {
                return string.Concat(person.FirstName, " ", person.LastName);
            }
        }

        private async Task<string> GetPrinInvestigatorRoleAsync(string sourceValue, bool bypassCache)
        {
            if (string.IsNullOrEmpty(sourceValue))
            {
                return string.Empty;
            }
            var allContactRoles = await GetGuidValcodeAsync<ContactRoles>("CORE", "CONTACT.ROLES",
                (gcr, g) => new ContactRoles(g, gcr.ValInternalCodeAssocMember, gcr.ValExternalRepresentationAssocMember), bypassCache: bypassCache);

            var contact = allContactRoles.FirstOrDefault(cr => cr.Code.Equals(sourceValue, StringComparison.OrdinalIgnoreCase));
            if (contact == null)
            {
                throw new KeyNotFoundException(string.Format("Could not find a ConfigurationSettings source title for value: '{0}'.", sourceValue));
            }
            else
            {
                return contact.Description;
            }
        }

        #endregion
    }
}