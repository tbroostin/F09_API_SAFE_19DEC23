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
    public class DefaultSettingsRepository : BaseColleagueRepository, IDefaultSettingsRepository
    {
        public RepositoryException exception = new RepositoryException();
        public static char _VM = Convert.ToChar(DynamicArray.VM);
        public static char _SM = Convert.ToChar(DynamicArray.SM);
        private readonly int _readSize;

        public DefaultSettingsRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            CacheTimeout = Level1CacheTimeoutValue;
            this._readSize = ((apiSettings != null) && (apiSettings.BulkReadSize > 0)) ? apiSettings.BulkReadSize : 5000;
        }

        /// <summary>
        /// Get a collection of IntgDefaultSettings
        /// </summary>
        /// <param name="bypassCache">Bypass cache flag</param>
        /// <returns>Collection of IntgDefaultSettings</returns>
        public async Task<IEnumerable<DefaultSettings>> GetDefaultSettingsAsync(bool bypassCache)
        {
            if (bypassCache && ContainsKey(BuildFullCacheKey("AllIntgDefaultSettings")))
            {
                ClearCache(new List<string> { "AllIntgDefaultSettings" });
            }
            return await GetOrAddToCacheAsync<IEnumerable<DefaultSettings>>("AllIntgDefaultSettings",
                async () =>
                {
                    var defaultSettingsIds = await DataReader.SelectAsync("INTG.DEFAULT.SETTINGS", "");
                    var defaultSettingsDataContracts = await DataReader.BulkReadRecordAsync<IntgDefaultSettings>("INTG.DEFAULT.SETTINGS", defaultSettingsIds);
                    return await BuildDefaultSettingsAsync(defaultSettingsDataContracts, bypassCache);

                }, Level1CacheTimeoutValue);
        }


        /// <summary>
        /// Get a single Configuration Settings domain entity from an academic credential guid.
        /// </summary>
        /// <param name="guid">Guid for Lookup</param>
        /// <param name="bypassCache"> Bypass Cache flag</param>
        /// <returns></returns>
        public async Task<DefaultSettings> GetDefaultSettingsByGuidAsync(string guid, bool bypassCache = false)
        {
            var intgDefaultSettings = await GetDefaultSettingsAsync(bypassCache);
            if (intgDefaultSettings == null || !intgDefaultSettings.Any())
            {
                throw new KeyNotFoundException(string.Format("No default-settings resource was found for GUID '{0}'.", guid));
            }
            var defaultSetting = intgDefaultSettings.FirstOrDefault(cs => cs.Guid.Equals(guid, StringComparison.InvariantCultureIgnoreCase));
            if (defaultSetting == null)
            {
                return await GetDefaultSettingsByIdAsync(await GetDefaultSettingsIdFromGuidAsync(guid), bypassCache);
            }
            return defaultSetting;
        }


        /// <summary>
        /// Get the record key from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>Primary key</returns>
        public async Task<string> GetDefaultSettingsIdFromGuidAsync(string guid)
        {

            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }
            var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(guid) });
            if (idDict == null || idDict.Count == 0)
            {
                throw new KeyNotFoundException("INTG.DEFAULT.SETTINGS GUID " + guid + " not found.");
            }
            var foundEntry = idDict.FirstOrDefault();
            if (foundEntry.Value == null || foundEntry.Value.Entity != "INTG.DEFAULT.SETTINGS" || !string.IsNullOrEmpty(foundEntry.Value.SecondaryKey))
            {
                throw new KeyNotFoundException(string.Format("The GUID specified: '{0}' is used by a different entity/secondary key: {1}", guid, foundEntry.Value != null ? foundEntry.Value.Entity : string.Empty));
            }
            return foundEntry.Value.PrimaryKey;
        }


        /// <summary>
        /// Get a single Configuration Settings domain entity from an academic credential id.
        /// </summary>
        /// <param name="id">The DefaultSettings id</param>
        /// <param name="bypassCache">Bypass Cache flag</param>
        /// <returns>Configuration Settings domain entity object</returns>
        private async Task<DefaultSettings> GetDefaultSettingsByIdAsync(string id, bool bypassCache)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID is required to get a AcademicCredentials.");
            }

            var intgDefaultSettings = await DataReader.ReadRecordAsync<IntgDefaultSettings>(id);
            if (intgDefaultSettings == null)
            {
                throw new KeyNotFoundException(string.Concat("Record not found, or AcademicCredentials with ID ", id, "invalid."));
            }

            var defaultSettings = await BuildDefaultSettingsAsync(intgDefaultSettings, bypassCache);

            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }

            return defaultSettings;
        }

        /// <summary>
        /// Get a collection of IntgDefaultSettings
        /// </summary>
        /// <param name="bypassCache">Bypass cache flag</param>
        /// <returns>Collection of IntgDefaultSettings</returns>
        public async Task<DefaultSettingsAdvancedSearch> GetDefaultSettingsAdvancedSearchOptionsAsync(string defaultSettingsId, string keyword, bool bypassCache)
        {
            DefaultSettingsAdvancedSearch defaultSettingsAdvancedSearch = null;
            var optionsCollection = new List<DefaultSettingsAdvancedSearchOptions>();
            if (!string.IsNullOrEmpty(defaultSettingsId) && !string.IsNullOrEmpty(keyword))
            {
                var advancedSearchRequest = BuildAdvancedSearchRequest(defaultSettingsId, keyword);
                var advancedSearchResponse = await transactionInvoker.ExecuteAsync<SearchDefaultSettingsRequest, SearchDefaultSettingsResponse>(advancedSearchRequest);
                if (advancedSearchResponse.DefaultSettingAdvancedSearchOptionsErrors.Any())
                {
                    var errorMessage = string.Format("Error(s) occurred searching for default-settings '{0}'.", defaultSettingsId);
                    var searchException = new RepositoryException(errorMessage);
                    advancedSearchResponse.DefaultSettingAdvancedSearchOptionsErrors.ForEach(e => searchException.AddError(new RepositoryError("Validation.Exception", string.Concat(e.ErrorCodes, ": ", e.ErrorMessages))
                    {
                        SourceId = defaultSettingsId
                    }
                    ));
                    logger.Error(errorMessage);
                    throw searchException;
                }
                if (advancedSearchResponse.MatchingData.Any())
                {                    
                    foreach (var match in advancedSearchResponse.MatchingData)
                    {
                        var option = new DefaultSettingsAdvancedSearchOptions(match.Titles, match.Values, match.Origins);
                        optionsCollection.Add(option);
                    }
                    defaultSettingsAdvancedSearch = new DefaultSettingsAdvancedSearch(defaultSettingsId, optionsCollection);
                }

            }
            return defaultSettingsAdvancedSearch;
        }

        /// <summary>
        /// Create an ApplicationSupportingItemRequest from a domain entity
        /// </summary>
        /// <param name="defaultSettingsEntity">supporting item domain entity</param>
        /// <returns>UpdateApplicationSupportingItemRequest transaction object</returns>
        private SearchDefaultSettingsRequest BuildAdvancedSearchRequest(string defaultSettingsId, string keyword)
        {
            var request = new SearchDefaultSettingsRequest()
            {
                Keyword = keyword,
                DefaultId = defaultSettingsId                
            };

            return request;
        }

        #region Update DefaultSettings

        public async Task<DefaultSettings> UpdateDefaultSettingsAsync(DefaultSettings defaultSettingss)
        {
            if (defaultSettingss == null)
            {
                throw new ArgumentNullException("defaultSettingssEntity", "Must provide a defaultSettingssEntityEntity to update.");
            }

            if (string.IsNullOrEmpty(defaultSettingss.Guid))
            {
                throw new ArgumentNullException("defaultSettingssEntity", "Must provide the guid of the defaultSettingssEntityEntity to update.");
            }

            // verify the GUID exists to perform an update.  If not, perform a create instead
            var defaultSettingssId = await GetDefaultSettingsIdFromGuidAsync(defaultSettingss.Guid);

            if (defaultSettingssId != null && !string.IsNullOrEmpty(defaultSettingssId))
            {
                var updateRequest = BuildUpdateRequest(defaultSettingss);

                // write the  data
                var updateResponse = await transactionInvoker.ExecuteAsync<UpdateDefaultSettingsRequest, UpdateDefaultSettingsResponse>(updateRequest);

                if (updateResponse.DefaultSettingErrors.Any())
                {
                    var errorMessage = string.Format("Error(s) occurred updating default-settings '{0}'.", defaultSettingss.Guid);
                    var exception = new RepositoryException(errorMessage);
                    updateResponse.DefaultSettingErrors.ForEach(e => exception.AddError(new RepositoryError("Validation.Exception", string.Concat(e.ErrorCodes, ": ", e.ErrorMessages))
                        {
                            Id = defaultSettingss.Guid,
                            SourceId = defaultSettingssId
                        }
                    ));
                    logger.Error(errorMessage);
                    throw exception;
                }
            }
            // get the updated entity from the database
            return await GetDefaultSettingsByGuidAsync(defaultSettingss.Guid, true);
        }

        /// <summary>
        /// Create an ApplicationSupportingItemRequest from a domain entity
        /// </summary>
        /// <param name="defaultSettingsEntity">supporting item domain entity</param>
        /// <returns>UpdateApplicationSupportingItemRequest transaction object</returns>
        private UpdateDefaultSettingsRequest BuildUpdateRequest(DefaultSettings defaultSettingsEntity)
        {
            var request = new UpdateDefaultSettingsRequest()
            {
                Guid = defaultSettingsEntity.Guid,
                DefaultId = defaultSettingsEntity.Code,
                DefaultTitle = defaultSettingsEntity.Description,
                DefaultResources = new List<DefaultResources>(),
                Description = defaultSettingsEntity.FieldHelp,
                SourceTitle = defaultSettingsEntity.SourceTitle,
                SourceValue = defaultSettingsEntity.SourceValue,
                SearchType = defaultSettingsEntity.SearchType ?? null,
                SearchMinLength = defaultSettingsEntity.SearchMinLength ?? null
            };
            if (defaultSettingsEntity.EthosResources != null && defaultSettingsEntity.EthosResources.Any())
            {
                foreach (var resource in defaultSettingsEntity.EthosResources)
                {
                    request.DefaultResources.Add(new DefaultResources()
                    {
                        Resources = resource.Resource,
                        ResourcePropertyNames = resource.PropertyName
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

        #region Build DefaultSettings

        /// <summary>
        /// Build from a Collection of Data Contracts
        /// </summary>
        /// <param name="sources"></param>
        /// <returns></returns>
        private async Task<IEnumerable<DefaultSettings>> BuildDefaultSettingsAsync(Collection<IntgDefaultSettings> sources, bool bypassCache = false)
        {
            var defaultSettingsCollection = new List<DefaultSettings>();

            foreach (var source in sources)
            {
                defaultSettingsCollection.Add(await BuildDefaultSettingsAsync(source, bypassCache));
            }

            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }

            return defaultSettingsCollection.AsEnumerable();
        }

        /// <summary>
        /// Build from a single data contract
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private async Task<DefaultSettings> BuildDefaultSettingsAsync(IntgDefaultSettings source, bool bypassCache = false)
        {
            if (source == null)
            {
                exception.AddError(new RepositoryError("Bad.Data", "Selected, but can not read INTG.DEFAULT.SETTINGS record."));
                return null;
            }

            DefaultSettings defaultSettings = null;

            try
            {
                defaultSettings = new DefaultSettings(source.RecordGuid, source.Recordkey, source.IdsCollDefaultTitle)
                {
                    EthosResources = new List<DefaultSettingsResource>(),
                    FieldHelp = source.IdsCollDefaultDesc,
                    EntityName = source.IdsCollEntity,
                    FieldName = source.IdsCollLdmFieldName,
                    ValcodeTableName = source.IdsCollValcodeId,
                    SearchType = source.IdsSearchType,
                    SearchMinLength = source.IdsSearchMinLength
                };
                if (source.DefaultResourcesEntityAssociation != null && source.DefaultResourcesEntityAssociation.Any())
                {
                    foreach (var resource in source.DefaultResourcesEntityAssociation)
                    {
                        if (!string.IsNullOrEmpty(resource.IdsEthosResourcesAssocMember))
                        {
                            defaultSettings.EthosResources.Add(new DefaultSettingsResource()
                            {
                                Resource = resource.IdsEthosResourcesAssocMember,
                                PropertyName = resource.IdsEthosPropertyNamesAssocMember
                            });
                        }
                    }
                }
                else
                {
                    exception.AddError(new RepositoryError("Missing.Required.Property", "Invalid INTG.DEFAULT.SETTINGS record.  Missing ethos.resource property.")
                    {
                        Id = source.RecordGuid,
                        SourceId = source.Recordkey
                    });
                }
                defaultSettings = await BuildSourceTitleAndValue(defaultSettings, source, bypassCache);
            }
            catch (Exception ex)
            {
                exception.AddError(new RepositoryError("Missing.Required.Property", "Invalid INTG.DEFAULT.SETTINGS record.  Missing one or more required property.  " + ex.Message)
                {
                    Id = source.RecordGuid,
                    SourceId = source.Recordkey
                });
            }

            return defaultSettings;
        }

        private async Task<DefaultSettings> BuildSourceTitleAndValue(DefaultSettings defaultSettings, IntgDefaultSettings defaultSettingsDataContract, bool bypassCache = false)
        {
            if (defaultSettings == null)
            {
                throw new KeyNotFoundException("Could not find a DefaultSettings record.");
            }

            var ldmDefaults = await GetLdmDefaults(bypassCache);
            var ldmFieldNames = defaultSettingsDataContract.IdsCollLdmFieldName.Split(',');
            var ldmFieldName = ldmFieldNames[0];

            switch (ldmFieldName.ToUpperInvariant())
            {
                case "LDMD.DEFAULT.PRIVACY.CODE":
                    {
                        defaultSettings.SourceValue = ldmDefaults.LdmdDefaultPrivacyCode;
                        defaultSettings.SourceTitle = await GetPrivacyCodeAsync(defaultSettings.SourceValue, bypassCache);
                        break;
                    }
                case "LDMD.CHARGE.TYPES":
                    {
                        if (ldmFieldNames.Count() > 2)
                        {
                            var arCodes = ldmDefaults.LdmdDefaultArCodes;
                            var arCode = string.Empty;
                            foreach (var chargeTypeAssoc in ldmDefaults.LdmdArDefaultsEntityAssociation)
                            {
                                if (chargeTypeAssoc.LdmdChargeTypesAssocMember == ldmFieldNames[2])
                                {
                                    arCode = chargeTypeAssoc.LdmdDefaultArCodesAssocMember;
                                }
                            }
                            if (!string.IsNullOrEmpty(arCode))
                            {
                                defaultSettings.SourceValue = arCode;
                                defaultSettings.SourceTitle = await GetArCodesAsync(defaultSettings.SourceValue, bypassCache);
                            }
                        }
                        break;
                    }
                case "LDMD.COLL.FIELD.NAME":
                    {
                        switch (defaultSettingsDataContract.IdsCollEntity)
                        {
                            case "CORP.FOUNDS":
                                foreach (var entity in ldmDefaults.LdmdCollDefaultsEntityAssociation)
                                {
                                    if (entity.LdmdCollFieldNameAssocMember == "CRS.APPROVAL.AGENCY.IDS")
                                    {
                                        defaultSettings.SourceValue = entity.LdmdCollDefaultValueAssocMember;
                                        defaultSettings.SourceTitle = await GetPersonNameAsync(defaultSettings.SourceValue, bypassCache);
                                    }
                                }
                                break;
                            case "STAFF":
                                foreach (var entity in ldmDefaults.LdmdCollDefaultsEntityAssociation)
                                {
                                    if (entity.LdmdCollFieldNameAssocMember == "CRS.APPROVAL.IDS")
                                    {
                                        defaultSettings.SourceValue = entity.LdmdCollDefaultValueAssocMember;
                                        defaultSettings.SourceTitle = await GetPersonNameAsync(defaultSettings.SourceValue, bypassCache);
                                    }
                                }
                                break;
                            case "CRED.TYPES":
                                foreach (var entity in ldmDefaults.LdmdCollDefaultsEntityAssociation)
                                {
                                    if (entity.LdmdCollFieldNameAssocMember == "CRS.CRED.TYPE")
                                    {
                                        defaultSettings.SourceValue = entity.LdmdCollDefaultValueAssocMember;
                                        defaultSettings.SourceTitle = await GetCreditTypeAsync(defaultSettings.SourceValue, bypassCache);
                                    }
                                }
                                break;
                            case "ACAD.LEVELS":
                                foreach (var entity in ldmDefaults.LdmdCollDefaultsEntityAssociation)
                                {
                                    if (entity.LdmdCollFieldNameAssocMember == "CRS.ACAD.LEVEL")
                                    {
                                        defaultSettings.SourceValue = entity.LdmdCollDefaultValueAssocMember;
                                        defaultSettings.SourceTitle = await GetAcadLevelAsync(defaultSettings.SourceValue, bypassCache);
                                    }
                                }
                                break;
                            case "ST.VALCODES":
                                foreach (var entity in ldmDefaults.LdmdCollDefaultsEntityAssociation)
                                {
                                    if (defaultSettings.ValcodeTableName == "COURSE.LEVELS" && entity.LdmdCollFieldNameAssocMember == "CRS.LEVELS")
                                    {
                                        defaultSettings.SourceValue = entity.LdmdCollDefaultValueAssocMember;
                                        defaultSettings.SourceTitle = await GetCourseLevelAsync(defaultSettings.SourceValue, bypassCache);
                                    }
                                    if (defaultSettings.ValcodeTableName == "TEACHING.ARRANGEMENTS" && entity.LdmdCollFieldNameAssocMember == "CSF.TEACHING.ARRANGEMENT")
                                    {
                                        defaultSettings.SourceValue = entity.LdmdCollDefaultValueAssocMember;
                                        defaultSettings.SourceTitle = await GetTeachingArrangementAsync(defaultSettings.SourceValue, bypassCache);
                                    }
                                }
                                break;
                            case "ASGMT.CONTRACT.TYPES":
                                foreach (var entity in ldmDefaults.LdmdCollDefaultsEntityAssociation)
                                {
                                    if (entity.LdmdCollFieldNameAssocMember == "PAC.TYPE")
                                    {
                                        defaultSettings.SourceValue = entity.LdmdCollDefaultValueAssocMember;
                                        defaultSettings.SourceTitle = await GetAssignmentContractTypeAsync(defaultSettings.SourceValue, bypassCache);
                                    }
                                }
                                break;
                            case "POSITION":
                                foreach (var entity in ldmDefaults.LdmdCollDefaultsEntityAssociation)
                                {
                                    if (entity.LdmdCollFieldNameAssocMember == "PLPP.POSITION.ID")
                                    {
                                        defaultSettings.SourceValue = entity.LdmdCollDefaultValueAssocMember;
                                        defaultSettings.SourceTitle = await GetPositionAsync(defaultSettings.SourceValue, bypassCache);
                                    }
                                }
                                break;
                            case "LOAD.PERIODS":
                                foreach (var entity in ldmDefaults.LdmdCollDefaultsEntityAssociation)
                                {
                                    if (entity.LdmdCollFieldNameAssocMember == "PLP.LOAD.PERIOD")
                                    {
                                        defaultSettings.SourceValue = entity.LdmdCollDefaultValueAssocMember;
                                        defaultSettings.SourceTitle = await GetLoadPeriodAsync(defaultSettings.SourceValue, bypassCache);
                                    }
                                }
                                break;
                        }
                        break;
                    }
                case "LDMD.BENDED.CODE":
                    {
                        if (ldmFieldNames.Count() > 2)
                        {
                            var valuePos = int.Parse(ldmFieldNames[1]) - 1;
                            var subValuePos = int.Parse(ldmFieldNames[2]) - 1;
                            var bendedCodeList = ldmDefaults.LdmdBendedCode;
                            if (bendedCodeList != null && bendedCodeList.Count() > valuePos)
                            {
                                var typeCodeList = bendedCodeList.ElementAt(valuePos).Split(_SM);
                                if (typeCodeList.Count() > subValuePos)
                                {
                                    defaultSettings.SourceValue = typeCodeList[subValuePos];
                                    defaultSettings.SourceTitle = await GetBendedCodeAsync(defaultSettings.SourceValue, bypassCache);
                                }
                            }
                        }
                        break;
                    }
                case "LDMD.COURSE.ACT.STATUS":
                    {
                        defaultSettings.SourceValue = ldmDefaults.LdmdCourseActStatus;
                        defaultSettings.SourceTitle = await GetCourseStatusAsync(defaultSettings.SourceValue, bypassCache);
                        break;
                    }
                case "LDMD.COURSE.INACT.STATUS":
                    {
                        defaultSettings.SourceValue = ldmDefaults.LdmdCourseInactStatus;
                        defaultSettings.SourceTitle = await GetCourseStatusAsync(defaultSettings.SourceValue, bypassCache);
                        break;
                    }
                case "LDMD.DEFAULT.APPL.STAT.STAFF":
                    {
                        defaultSettings.SourceValue = ldmDefaults.LdmdDefaultApplStatStaff;
                        defaultSettings.SourceTitle = await GetPersonNameAsync(defaultSettings.SourceValue, bypassCache);
                        break;
                    }
                case "LDMD.DEFAULT.APPL.STATUS":
                    {
                        defaultSettings.SourceValue = ldmDefaults.LdmdDefaultApplStatus;
                        defaultSettings.SourceTitle = await GetApplicationStatusAsync(defaultSettings.SourceValue, bypassCache);
                        break;
                    }
                case "LDMD.PRSPCT.APPL.STAT.STAFF":
                    {
                        defaultSettings.SourceValue = ldmDefaults.LdmdPrspctApplStatStaff;
                        defaultSettings.SourceTitle = await GetPersonNameAsync(defaultSettings.SourceValue, bypassCache);
                        break;
                    }
                case "LDMD.PRSPCT.APPL.STATUS":
                    {
                        defaultSettings.SourceValue = ldmDefaults.LdmdPrspctApplStatus;
                        defaultSettings.SourceTitle = await GetApplicationStatusAsync(defaultSettings.SourceValue, bypassCache);
                        break;
                    }
                case "LDMD.DEFAULT.AR.TYPE":
                    {
                        defaultSettings.SourceValue = ldmDefaults.LdmdDefaultArType;
                        defaultSettings.SourceTitle = await GetArTypeAsync(defaultSettings.SourceValue, bypassCache);
                        break;
                    }
                case "LDMD.DEFAULT.DISTR":
                    {
                        defaultSettings.SourceValue = ldmDefaults.LdmdDefaultDistr;
                        defaultSettings.SourceTitle = await GetReceiptTenderGlDistrAsync(defaultSettings.SourceValue, bypassCache);
                        break;
                    }
                case "LDMD.SPONSOR.AR.CODE":
                    {
                        defaultSettings.SourceValue = ldmDefaults.LdmdSponsorArCode;
                        defaultSettings.SourceTitle = await GetArCodesAsync(defaultSettings.SourceValue, bypassCache);
                        break;
                    }
                case "LDMD.SPONSOR.AR.TYPE":
                    {
                        defaultSettings.SourceValue = ldmDefaults.LdmdSponsorArType;
                        defaultSettings.SourceTitle = await GetArTypeAsync(defaultSettings.SourceValue, bypassCache);
                        break;
                    }
                case "LDMD.DEFAULT.SPONSOR":
                    {
                        defaultSettings.SourceValue = ldmDefaults.LdmdDefaultSponsor;
                        defaultSettings.SourceTitle = await GetPersonNameAsync(defaultSettings.SourceValue, bypassCache);
                        break;
                    }
                case "LDMD.PAYMENT.METHOD":
                    {
                        defaultSettings.SourceValue = ldmDefaults.LdmdPaymentMethod;
                        defaultSettings.SourceTitle = await GetPaymentMethodAsync(defaultSettings.SourceValue, bypassCache);
                        break;
                    }
                case "LDMD.DFLT.RES.LIFE.AR.TYPE":
                    {
                        defaultSettings.SourceValue = ldmDefaults.LdmdDfltResLifeArType;
                        defaultSettings.SourceTitle = await GetArTypeAsync(defaultSettings.SourceValue, bypassCache);
                        break;
                    }
                case "LDMD.DEFAULT.AP.TYPE":
                    {
                        defaultSettings.SourceValue = ldmDefaults.LdmdDefaultApType;
                        defaultSettings.SourceTitle = await GetApTypeAsync(defaultSettings.SourceValue, bypassCache);
                        break;
                    }
            }

            return defaultSettings;
        }
        #endregion

        #region Helper Methods (Public)

        /// <summary>
        /// Retrieve all AR.CODES records from cache and return dictionary
        /// of key and description.
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns>"Dictionary of key value pair, key and description.</returns>
        public async Task<Dictionary<string, string>> GetAllArCodesAsync(bool bypassCache)
        {
            Dictionary<string, string> dictArCodes = new Dictionary<string, string>();

            var cacheControlKey = "AllArCodesDefaultSettings";
            if (bypassCache && ContainsKey(BuildFullCacheKey(cacheControlKey)))
            {
                ClearCache(new List<string> { cacheControlKey });
            }
            var allArCodes = await GetOrAddToCacheAsync<IEnumerable<ArCodesBase>>(cacheControlKey,
                async () =>
                {
                    var arCodes = await DataReader.BulkReadRecordAsync<ArCodesBase>("AR.CODES", "");
                    if (arCodes == null)
                    {
                        logger.Info("Unable to access AR.CODES from database.");
                        arCodes = new Collection<ArCodesBase>();
                    }
                    return arCodes;
                }, Level1CacheTimeoutValue);

            if (allArCodes != null && allArCodes.Any())
            {
               foreach (var arCode in allArCodes)
                {
                    if (arCode != null && !string.IsNullOrEmpty(arCode.ArcDesc))
                    {
                        var dictKey = arCode.Recordkey;
                        var dictValue = arCode.ArcDesc;
                        if (!string.IsNullOrEmpty(dictKey) && !dictArCodes.ContainsKey(dictKey))
                        {
                            dictArCodes.Add(dictKey, dictValue);
                        }
                    }
                }
            }

            return dictArCodes;
        }

        /// <summary>
        /// Retrieve all AR.TYPES records from cache and return dictionary
        /// of key and description.
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns>"Dictionary of key value pair, key and description.</returns>
        public async Task<Dictionary<string, string>> GetAllArTypesAsync(bool bypassCache)
        {
            Dictionary<string, string> dictArTypes = new Dictionary<string, string>();

            var cacheControlKey = "AllArTypesDefaultSettings";
            if (bypassCache && ContainsKey(BuildFullCacheKey(cacheControlKey)))
            {
                ClearCache(new List<string> { cacheControlKey });
            }
            var allArTypes = await GetOrAddToCacheAsync<IEnumerable<ArTypesBase>>(cacheControlKey,
                async () =>
                {
                    var arTypes = await DataReader.BulkReadRecordAsync<ArTypesBase>("AR.TYPES", "");
                    if (arTypes == null)
                    {
                        logger.Info("Unable to access AR.TYPES from database.");
                        arTypes = new Collection<ArTypesBase>();
                    }
                    return arTypes;
                }, Level1CacheTimeoutValue);

            if (allArTypes != null && allArTypes.Any())
            {
                foreach (var arCode in allArTypes)
                {
                    if (arCode != null && !string.IsNullOrEmpty(arCode.ArtDesc))
                    {
                        var dictKey = arCode.Recordkey;
                        var dictValue = arCode.ArtDesc;
                        if (!string.IsNullOrEmpty(dictKey) && !dictArTypes.ContainsKey(dictKey))
                        {
                            dictArTypes.Add(dictKey, dictValue);
                        }
                    }
                }
            }

            return dictArTypes;
        }


        /// <summary>
        /// Retrieve all AP.TYPES records from cache and return dictionary
        /// of key and description.
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns>"Dictionary of key value pair, key and description.</returns>
        public async Task<Dictionary<string, string>> GetAllApTypesAsync(bool bypassCache)
        {
            Dictionary<string, string> dictArTypes = new Dictionary<string, string>();

            var cacheControlKey = "AllApTypesDefaultSettings";
            if (bypassCache && ContainsKey(BuildFullCacheKey(cacheControlKey)))
            {
                ClearCache(new List<string> { cacheControlKey });
            }
            var allApTypes = await GetOrAddToCacheAsync<IEnumerable<ApTypesBase>>(cacheControlKey,
                async () =>
                {
                    var arTypes = await DataReader.BulkReadRecordAsync<ApTypesBase>("AP.TYPES", "WITH APT.SOURCE EQ 'R'");
                    if (arTypes == null)
                    {
                        logger.Info("Unable to access AP.TYPES from database.");
                        arTypes = new Collection<ApTypesBase>();
                    }
                    return arTypes;
                }, Level1CacheTimeoutValue);

            if (allApTypes != null && allApTypes.Any())
            {
                foreach (var arCode in allApTypes)
                {
                    if (arCode != null && !string.IsNullOrEmpty(arCode.ApTypesDesc))
                    {
                        var dictKey = arCode.Recordkey;
                        var dictValue = arCode.ApTypesDesc;
                        if (!string.IsNullOrEmpty(dictKey) && !dictArTypes.ContainsKey(dictKey))
                        {
                            dictArTypes.Add(dictKey, dictValue);
                        }
                    }
                }
            }

            return dictArTypes;
        }

        /// <summary>
        /// Retrieve all CORP.FOUNDS records used as approval agencies from cache and return dictionary
        /// of key and description.
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns>"Dictionary of key value pair, key and description.</returns>
        public async Task<Dictionary<string, string>> GetAllApprovalAgenciesAsync(bool bypassCache)
        {
            Dictionary<string, string> dictApprovalAgencies = new Dictionary<string, string>();

            var cacheControlKey = "AllApprovalAgenciesDefaultSettings";
            if (bypassCache && ContainsKey(BuildFullCacheKey(cacheControlKey)))
            {
                ClearCache(new List<string> { cacheControlKey });
            }
            var allApprovalAgencies = await GetOrAddToCacheAsync<IEnumerable<DataContracts.Person>>(cacheControlKey,
                async () =>
                {
                    string criteria = "WITH CRS.APPROVAL.AGENCY.IDS NE '' BY.EXP CRS.APPROVAL.AGENCY.IDS SAVING CRS.APPROVAL.AGENCY.IDS";
                    var approvalAgencyIds = await DataReader.SelectAsync("COURSES", criteria);
                    if (approvalAgencyIds == null || !approvalAgencyIds.Any())
                    {
                        return new Collection<DataContracts.Person>();
                    }
                    
                    var corpFounds = await DataReader.BulkReadRecordAsync<DataContracts.Person>("PERSON", approvalAgencyIds);
                    if (corpFounds == null)
                    {
                        logger.Info("Unable to access PERSON from database.");
                        corpFounds = new Collection<DataContracts.Person>();
                    }
                    return corpFounds;
                }, Level1CacheTimeoutValue);

            if (allApprovalAgencies != null && allApprovalAgencies.Any())
            {
                foreach (var person in allApprovalAgencies)
                {
                    if (person != null && !string.IsNullOrEmpty(person.LastName))
                    {
                        var dictKey = person.Recordkey;
                        var dictValue = string.IsNullOrEmpty(person.PreferredName) ? person.LastName : person.PreferredName;
                        if (!string.IsNullOrEmpty(dictKey) && !dictApprovalAgencies.ContainsKey(dictKey))
                        {
                            dictApprovalAgencies.Add(dictKey, dictValue);
                        }
                    }
                }
            }

            return dictApprovalAgencies;
        }

        /// <summary>
        /// Retrieve all STAFF records used as approvals from cache and return dictionary
        /// of key and description.
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns>"Dictionary of key value pair, key and description.</returns>
        public async Task<Dictionary<string, string>> GetAllStaffApprovalsAsync(bool bypassCache)
        {
            Dictionary<string, string> dictStaff = new Dictionary<string, string>();

            var cacheControlKey = "AllStaffApprovalsDefaultSettings";
            if (bypassCache && ContainsKey(BuildFullCacheKey(cacheControlKey)))
            {
                ClearCache(new List<string> { cacheControlKey });
            }
            var allStaffApproval = await GetOrAddToCacheAsync<IEnumerable<DataContracts.Person>>(cacheControlKey,
                async () =>
                {
                    string criteria = "WITH CRS.APPROVAL.IDS NE '' BY.EXP CRS.APPROVAL.IDS SAVING CRS.APPROVAL.IDS";
                    var staffApprovalIds = await DataReader.SelectAsync("COURSES", criteria);
                    if (staffApprovalIds == null || !staffApprovalIds.Any())
                    {
                        return new Collection<DataContracts.Person>();
                    }

                    var staffApproval = await DataReader.BulkReadRecordAsync<DataContracts.Person>("PERSON", staffApprovalIds);
                    if (staffApproval == null)
                    {
                        logger.Info("Unable to access PERSON from database.");
                        staffApproval = new Collection<DataContracts.Person>();
                    }
                    return staffApproval;
                }, Level1CacheTimeoutValue);

            if (allStaffApproval != null && allStaffApproval.Any())
            {
                foreach (var person in allStaffApproval)
                {
                    if (person != null && !string.IsNullOrEmpty(person.LastName))
                    {
                        var dictKey = person.Recordkey;
                        var dictValue = string.Concat(person.FirstName, " ", person.LastName).Trim();
                        if (!string.IsNullOrEmpty(dictKey) && !string.IsNullOrEmpty(dictValue) && !dictStaff.ContainsKey(dictKey))
                        {
                            dictStaff.Add(dictKey, dictValue);
                        }
                    }
                }
            }

            return dictStaff;
        }

        /// <summary>
        /// Retrieve all CRED.TYPES records from cache and return dictionary
        /// of key and description.
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns>"Dictionary of key value pair, key and description.</returns>
        public async Task<Dictionary<string, string>> GetAllCreditTypesAsync(bool bypassCache)
        {
            Dictionary<string, string> dictCreditTypes = new Dictionary<string, string>();

            var cacheControlKey = "AllCreditTypesDefaultSettings";
            if (bypassCache && ContainsKey(BuildFullCacheKey(cacheControlKey)))
            {
                ClearCache(new List<string> { cacheControlKey });
            }
            var allCreditTypes = await GetOrAddToCacheAsync<IEnumerable<CredTypesBase>>(cacheControlKey,
                async () =>
                {
                    var credTypes = await DataReader.BulkReadRecordAsync<CredTypesBase>("CRED.TYPES", "");
                    if (credTypes == null)
                    {
                        logger.Info("Unable to access CRED.TYPES from database.");
                        credTypes = new Collection<CredTypesBase>();
                    }
                    return credTypes;
                }, Level1CacheTimeoutValue);

            if (allCreditTypes != null && allCreditTypes.Any())
            {
                foreach (var credType in allCreditTypes)
                {
                    if (credType != null && !string.IsNullOrEmpty(credType.CrtpDesc))
                    {
                        var dictKey = credType.Recordkey;
                        var dictValue = credType.CrtpDesc;
                        if (!string.IsNullOrEmpty(dictKey) && !dictCreditTypes.ContainsKey(dictKey))
                        {
                            dictCreditTypes.Add(dictKey, dictValue);
                        }
                    }
                }
            }

            return dictCreditTypes;
        }

        /// <summary>
        /// Retrieve all ACAD.LEVELS records from cache and return dictionary
        /// of key and description.
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns>"Dictionary of key value pair, key and description.</returns>
        public async Task<Dictionary<string, string>> GetAllAcademicLevelsAsync(bool bypassCache)
        {
            Dictionary<string, string> dictAcademicLevels = new Dictionary<string, string>();

            var cacheControlKey = "AllAcademicLevelsDefaultSettings";
            if (bypassCache && ContainsKey(BuildFullCacheKey(cacheControlKey)))
            {
                ClearCache(new List<string> { cacheControlKey });
            }
            var allAcadLevels = await GetOrAddToCacheAsync<IEnumerable<AcadLevelsBase>>(cacheControlKey,
                async () =>
                {
                    var credTypes = await DataReader.BulkReadRecordAsync<AcadLevelsBase>("ACAD.LEVELS", "");
                    if (credTypes == null)
                    {
                        logger.Info("Unable to access ACAD.LEVELS from database.");
                        credTypes = new Collection<AcadLevelsBase>();
                    }
                    return credTypes;
                }, Level1CacheTimeoutValue);

            if (allAcadLevels != null && allAcadLevels.Any())
            {
                foreach (var acadLevel in allAcadLevels)
                {
                    if (acadLevel != null && !string.IsNullOrEmpty(acadLevel.AclvDesc))
                    {
                        var dictKey = acadLevel.Recordkey;
                        var dictValue = acadLevel.AclvDesc;
                        if (!string.IsNullOrEmpty(dictKey) && !dictAcademicLevels.ContainsKey(dictKey))
                        {
                            dictAcademicLevels.Add(dictKey, dictValue);
                        }
                    }
                }
            }

            return dictAcademicLevels;
        }

        /// <summary>
        /// Retrieve all ASGMT.CONTRACT.TYPES records from cache and return dictionary
        /// of key and description.
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns>"Dictionary of key value pair, key and description.</returns>
        public async Task<Dictionary<string, string>> GetAllAssignmentContractTypesAsync(bool bypassCache)
        {
            Dictionary<string, string> dictAssignmentContractTypes = new Dictionary<string, string>();

            var cacheControlKey = "AllAssignmentContractTypesDefaultSettings";
            if (bypassCache && ContainsKey(BuildFullCacheKey(cacheControlKey)))
            {
                ClearCache(new List<string> { cacheControlKey });
            }
            var allAssignmentContractTypes = await GetOrAddToCacheAsync<IEnumerable<AsgmtContractTypesBase>>(cacheControlKey,
                async () =>
                {
                    var assignmentContractTypes = await DataReader.BulkReadRecordAsync<AsgmtContractTypesBase>("ASGMT.CONTRACT.TYPES", "");
                    if (assignmentContractTypes == null)
                    {
                        logger.Info("Unable to access ASGMT.CONTRACT.TYPES from database.");
                        assignmentContractTypes = new Collection<AsgmtContractTypesBase>();
                    }
                    return assignmentContractTypes;
                }, Level1CacheTimeoutValue);

            if (allAssignmentContractTypes != null && allAssignmentContractTypes.Any())
            {
                foreach (var assignmentContractType in allAssignmentContractTypes)
                {
                    if (assignmentContractType != null && !string.IsNullOrEmpty(assignmentContractType.ActypDesc))
                    {
                        var dictKey = assignmentContractType.Recordkey;
                        var dictValue = assignmentContractType.ActypDesc;
                        if (!string.IsNullOrEmpty(dictKey) && !dictAssignmentContractTypes.ContainsKey(dictKey))
                        {
                            dictAssignmentContractTypes.Add(dictKey, dictValue);
                        }
                    }
                }
            }

            return dictAssignmentContractTypes;
        }

        /// <summary>
        /// Retrieve all POSITION records from cache and return dictionary
        /// of key and description.
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns>"Dictionary of key value pair, key and description.</returns>
        public async Task<Dictionary<string, string>> GetAllPositionsAsync(bool bypassCache)
        {
            Dictionary<string, string> dictPositions = new Dictionary<string, string>();

            var cacheControlKey = "AllPositionsDefaultSettings";
            if (bypassCache && ContainsKey(BuildFullCacheKey(cacheControlKey)))
            {
                ClearCache(new List<string> { cacheControlKey });
            }
            var allPositions = await GetOrAddToCacheAsync<IEnumerable<PositionBase>>(cacheControlKey,
                async () =>
                {
                    var today = await GetUnidataFormatDateAsync(DateTime.Now);
                    string criteria = string.Format("WITH POS.END.DATE EQ '' OR POS.END.DATE GT '{0}'", today);
                    var positions = await DataReader.BulkReadRecordAsync<PositionBase>("POSITION", criteria);
                    if (positions == null)
                    {
                        logger.Info("Unable to access POSITION from database.");
                        positions = new Collection<PositionBase>();
                    }
                    return positions;
                }, Level1CacheTimeoutValue);

            if (allPositions != null && allPositions.Any())
            {
                foreach (var position in allPositions)
                {
                    if (position != null && !string.IsNullOrEmpty(position.PosTitle))
                    {
                        var dictKey = position.Recordkey;
                        var dictValue = position.PosTitle;
                        if (!string.IsNullOrEmpty(dictKey) && !dictPositions.ContainsKey(dictKey))
                        {
                            dictPositions.Add(dictKey, dictValue);
                        }
                    }
                }
            }

            return dictPositions;
        }

        /// <summary>
        /// Retrieve all LOAD.PERIODS records from cache and return dictionary
        /// of key and description.
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns>"Dictionary of key value pair, key and description.</returns>
        public async Task<Dictionary<string, string>> GetAllLoadPeriodsAsync(bool bypassCache)
        {
            Dictionary<string, string> dictLoadPeriods = new Dictionary<string, string>();

            var cacheControlKey = "AllLoadPeriodsDefaultSettings";
            if (bypassCache && ContainsKey(BuildFullCacheKey(cacheControlKey)))
            {
                ClearCache(new List<string> { cacheControlKey });
            }
            var allLoadPeriods = await GetOrAddToCacheAsync<IEnumerable<LoadPeriodsBase>>(cacheControlKey,
                async () =>
                {
                    var loadPeriods = await DataReader.BulkReadRecordAsync<LoadPeriodsBase>("LOAD.PERIODS", "");
                    if (loadPeriods == null)
                    {
                        logger.Info("Unable to access LOAD.PERIODS from database.");
                        loadPeriods = new Collection<LoadPeriodsBase>();
                    }
                    return loadPeriods;
                }, Level1CacheTimeoutValue);

            if (allLoadPeriods != null && allLoadPeriods.Any())
            {
                foreach (var loadPeriod in allLoadPeriods)
                {
                    if (loadPeriod != null && !string.IsNullOrEmpty(loadPeriod.LdpdDesc))
                    {
                        var dictKey = loadPeriod.Recordkey;
                        var dictValue = loadPeriod.LdpdDesc;
                        if (!string.IsNullOrEmpty(dictKey) && !dictLoadPeriods.ContainsKey(dictKey))
                        {
                            dictLoadPeriods.Add(dictKey, dictValue);
                        }
                    }
                }
            }

            return dictLoadPeriods;
        }

        /// <summary>
        /// Retrieve all BENDED records from cache and return dictionary
        /// of key and description.
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns>"Dictionary of key value pair, key and description.</returns>
        public async Task<Dictionary<string, string>> GetAllBendedCodesAsync(bool bypassCache)
        {
            Dictionary<string, string> dictBendedCodes = new Dictionary<string, string>();

            var cacheControlKey = "AllBendedCodesDefaultSettings";
            if (bypassCache && ContainsKey(BuildFullCacheKey(cacheControlKey)))
            {
                ClearCache(new List<string> { cacheControlKey });
            }
            var allBendedCodes = await GetOrAddToCacheAsync<IEnumerable<BendedBase>>(cacheControlKey,
                async () =>
                {
                    var BendedCodes = await DataReader.BulkReadRecordAsync<BendedBase>("BENDED", "WITH BD.CALC.METHOD = 'A''R'");
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

            return dictBendedCodes;
        }

        /// <summary>
        /// Retrieve all STAFF records used as application staff from cache and return dictionary
        /// of key and description.
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns>"Dictionary of key value pair, key and description.</returns>
        public async Task<Dictionary<string, string>> GetAllApplicationStaffAsync(bool bypassCache)
        {
            Dictionary<string, string> dictStaff = new Dictionary<string, string>();

            var cacheControlKey = "AllApplicationStaffDefaultSettings";
            if (bypassCache && ContainsKey(BuildFullCacheKey(cacheControlKey)))
            {
                ClearCache(new List<string> { cacheControlKey });
            }
            var allStaffApproval = await GetOrAddToCacheAsync<IEnumerable<DataContracts.Person>>(cacheControlKey,
                async () =>
                {
                    string criteria = "WITH APPL.DECISION.BY NE '' BY.EXP APPL.DECISION.BY SAVING APPL.DECISION.BY";
                    var staffApprovalIds = await DataReader.SelectAsync("APPLICATIONS", criteria);
                    if (staffApprovalIds == null || !staffApprovalIds.Any())
                    {
                        return new Collection<DataContracts.Person>();
                    }

                    var staffApproval = await DataReader.BulkReadRecordAsync<DataContracts.Person>("PERSON", staffApprovalIds);
                    if (staffApproval == null)
                    {
                        logger.Info("Unable to access PERSON from database.");
                        staffApproval = new Collection<DataContracts.Person>();
                    }
                    return staffApproval;
                }, Level1CacheTimeoutValue);

            if (allStaffApproval != null && allStaffApproval.Any())
            {
                foreach (var person in allStaffApproval)
                {
                    if (person != null && !string.IsNullOrEmpty(person.LastName))
                    {
                        var dictKey = person.Recordkey;
                        var dictValue = string.Concat(person.FirstName, " ", person.LastName).Trim();
                        if (!string.IsNullOrEmpty(dictKey) && !string.IsNullOrEmpty(dictValue) && !dictStaff.ContainsKey(dictKey))
                        {
                            dictStaff.Add(dictKey, dictValue);
                        }
                    }
                }
            }

            return dictStaff;
        }

        /// <summary>
        /// Retrieve all APPLICATION.STATUSES records from cache and return dictionary
        /// of key and description.
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns>"Dictionary of key value pair, key and description.</returns>
        public async Task<Dictionary<string, string>> GetAllApplicationStatusesAsync(bool bypassCache)
        {
            Dictionary<string, string> dictLoadPeriods = new Dictionary<string, string>();

            var cacheControlKey = "AllApplicationStatusesDefaultSettings";
            if (bypassCache && ContainsKey(BuildFullCacheKey(cacheControlKey)))
            {
                ClearCache(new List<string> { cacheControlKey });
            }
            var allApplicationStatuses = await GetOrAddToCacheAsync<IEnumerable<ApplicationStatusesBase>>(cacheControlKey,
                async () =>
                {
                    var loadPeriods = await DataReader.BulkReadRecordAsync<ApplicationStatusesBase>("APPLICATION.STATUSES", "");
                    if (loadPeriods == null)
                    {
                        logger.Info("Unable to access APPLICATION.STATUSES from database.");
                        loadPeriods = new Collection<ApplicationStatusesBase>();
                    }
                    return loadPeriods;
                }, Level1CacheTimeoutValue);

            if (allApplicationStatuses != null && allApplicationStatuses.Any())
            {
                foreach (var applStatus in allApplicationStatuses)
                {
                    if (applStatus != null && !string.IsNullOrEmpty(applStatus.AppsDesc))
                    {
                        var dictKey = applStatus.Recordkey;
                        var dictValue = applStatus.AppsDesc;
                        if (!string.IsNullOrEmpty(dictKey) && !dictLoadPeriods.ContainsKey(dictKey))
                        {
                            dictLoadPeriods.Add(dictKey, dictValue);
                        }
                    }
                }
            }

            return dictLoadPeriods;
        }

        /// <summary>
        /// Retrieve all RCPT.TENDER.GL.DISTR records from cache and return dictionary
        /// of key and description.
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns>"Dictionary of key value pair, key and description.</returns>
        public async Task<Dictionary<string, string>> GetAllReceiptTenderGlDistrsAsync(bool bypassCache)
        {
            Dictionary<string, string> dictReceiptTenderGlDistr = new Dictionary<string, string>();

            var cacheControlKey = "AllReceiptTenderGlDistrDefaultSettings";
            if (bypassCache && ContainsKey(BuildFullCacheKey(cacheControlKey)))
            {
                ClearCache(new List<string> { cacheControlKey });
            }
            var allReceiptTenderGlDistr = await GetOrAddToCacheAsync<IEnumerable<RcptTenderGlDistrBase>>(cacheControlKey,
                async () =>
                {
                    var receiptTenderGlDistrs = await DataReader.BulkReadRecordAsync<RcptTenderGlDistrBase>("RCPT.TENDER.GL.DISTR", "");
                    if (receiptTenderGlDistrs == null)
                    {
                        logger.Info("Unable to access RCPT.TENDER.GL.DISTR from database.");
                        receiptTenderGlDistrs = new Collection<RcptTenderGlDistrBase>();
                    }
                    return receiptTenderGlDistrs;
                }, Level1CacheTimeoutValue);

            if (allReceiptTenderGlDistr != null && allReceiptTenderGlDistr.Any())
            {
                foreach (var receiptTenderGlDistr in allReceiptTenderGlDistr)
                {
                    if (receiptTenderGlDistr != null && !string.IsNullOrEmpty(receiptTenderGlDistr.RcpttDesc))
                    {
                        var dictKey = receiptTenderGlDistr.Recordkey;
                        var dictValue = receiptTenderGlDistr.RcpttDesc;
                        if (!string.IsNullOrEmpty(dictKey) && !dictReceiptTenderGlDistr.ContainsKey(dictKey))
                        {
                            dictReceiptTenderGlDistr.Add(dictKey, dictValue);
                        }
                    }
                }
            }

            return dictReceiptTenderGlDistr;
        }

        /// <summary>
        /// Retrieve all PAYMENT.METHOD records from cache and return dictionary
        /// of key and description.
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns>"Dictionary of key value pair, key and description.</returns>
        public async Task<Dictionary<string, string>> GetAllPaymentMethodsAsync(bool bypassCache)
        {
            Dictionary<string, string> dictPaymentMethod = new Dictionary<string, string>();

            var cacheControlKey = "AllPaymentMethodDefaultSettings";
            if (bypassCache && ContainsKey(BuildFullCacheKey(cacheControlKey)))
            {
                ClearCache(new List<string> { cacheControlKey });
            }
            var allPaymentMethod = await GetOrAddToCacheAsync<IEnumerable<PaymentMethods>>(cacheControlKey,
                async () =>
                {
                    var paymentMethods = await DataReader.BulkReadRecordAsync<PaymentMethods>("PAYMENT.METHOD", "WITH PMTH.ECOMM.ENABLED.FLAG NE 'Y'");
                    if (paymentMethods == null)
                    {
                        logger.Info("Unable to access PAYMENT.METHOD from database.");
                        paymentMethods = new Collection<PaymentMethods>();
                    }
                    return paymentMethods;
                }, Level1CacheTimeoutValue);

            if (allPaymentMethod != null && allPaymentMethod.Any())
            {
                foreach (var paymentMethod in allPaymentMethod)
                {
                    if (paymentMethod != null && !string.IsNullOrEmpty(paymentMethod.PmthDescription))
                    {
                        var dictKey = paymentMethod.Recordkey;
                        var dictValue = paymentMethod.PmthDescription;
                        if (!string.IsNullOrEmpty(dictKey) && !dictPaymentMethod.ContainsKey(dictKey))
                        {
                            dictPaymentMethod.Add(dictKey, dictValue);
                        }
                    }
                }
            }

            return dictPaymentMethod;
        }

        /// <summary>
        /// Retrieve all STAFF records used as application staff from cache and return dictionary
        /// of key and description.
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns>"Dictionary of key value pair, key and description.</returns>
        public async Task<Dictionary<string, string>> GetAllSponsorsAsync(bool bypassCache)
        {
            Dictionary<string, string> dictStaff = new Dictionary<string, string>();

            var cacheControlKey = "AllSponsorsDefaultSettings";
            if (bypassCache && ContainsKey(BuildFullCacheKey(cacheControlKey)))
            {
                ClearCache(new List<string> { cacheControlKey });
            }
            var allSponsors = await GetOrAddToCacheAsync<IEnumerable<DataContracts.Person>>(cacheControlKey,
                async () =>
                {
                    string criteria = "WITH SECSPN.SPONSOR NE '' SAVING UNIQUE SECSPN.SPONSOR";
                    var secSponsorIds = await DataReader.SelectAsync("SEC.SPONSORSHIPS", criteria);
                    criteria = "WITH PSPN.SPONSOR NE '' SAVING UNIQUE PSPN.SPONSOR";
                    var personSponsorIds = await DataReader.SelectAsync("PERSON.SPONSORSHIPS", criteria);

                    var sponsorIds = secSponsorIds.Union(personSponsorIds).Distinct().ToArray();
                    if (sponsorIds == null || !sponsorIds.Any())
                    {
                        return new Collection<DataContracts.Person>();
                    }

                    var staffApproval = await DataReader.BulkReadRecordAsync<DataContracts.Person>("PERSON", sponsorIds);
                    if (staffApproval == null)
                    {
                        logger.Info("Unable to access PERSON from database.");
                        staffApproval = new Collection<DataContracts.Person>();
                    }
                    return staffApproval;
                }, Level1CacheTimeoutValue);

            if (allSponsors != null && allSponsors.Any())
            {
                foreach (var person in allSponsors)
                {
                    if (person != null && !string.IsNullOrEmpty(person.LastName))
                    {
                        var dictKey = person.Recordkey;
                        var dictValue = string.Concat(person.FirstName, " ", person.LastName).Trim();
                        if (person.PersonCorpIndicator.ToUpperInvariant() == "Y")
                        {
                            dictValue = string.IsNullOrEmpty(person.PreferredName) ? person.LastName : person.PreferredName;
                        }
                        if (!string.IsNullOrEmpty(dictKey) && !string.IsNullOrEmpty(dictValue) && !dictStaff.ContainsKey(dictKey))
                        {
                            dictStaff.Add(dictKey, dictValue);
                        }
                    }
                }
            }

            return dictStaff;
        }

        public async Task<Dictionary<string, string>> GetAllValcodeItemsAsync(string entity, string valcodeTable, bool bypassCache, string specialProcessing = "*")
        {
            Dictionary<string, string> dictValcodeItems = new Dictionary<string, string>();

            var cacheControlKey = string.Concat("AllValcodeTableDefaultSettings", entity, valcodeTable, specialProcessing);
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
                            if (row.ValActionCode1AssocMember == specialProcessing || specialProcessing == "*")
                            {
                                var dictKey = row.ValInternalCodeAssocMember;
                                var dictValue = row.ValExternalRepresentationAssocMember;
                                if (!string.IsNullOrEmpty(dictKey) && !string.IsNullOrEmpty(dictValue) && !dictValcodeItems.ContainsKey(dictKey))
                                {
                                    dictValcodeItems.Add(dictKey, dictValue);
                                }
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

        private async Task<string> GetArCodesAsync(string sourceValue, bool bypassCache)
        {
            if (string.IsNullOrEmpty(sourceValue))
            {
                return string.Empty;
            }

            if (bypassCache)
            {
                var arCode = await DataReader.ReadRecordAsync<ArCodesBase>(sourceValue);
                if (arCode == null || string.IsNullOrEmpty(arCode.ArcDesc))
                {
                    throw new KeyNotFoundException(string.Format("Could not find a DefaultSettings source title for value: '{0}'.", sourceValue));
                }
                return arCode.ArcDesc;
            }
            var dictArCodes = await GetAllArCodesAsync(bypassCache);

            if (!dictArCodes.ContainsKey(sourceValue))
            {
                throw new KeyNotFoundException(string.Format("Could not find a DefaultSettings source title for value: '{0}'.", sourceValue));
            }
            else
            {
                string sourceTitle = string.Empty;
                dictArCodes.TryGetValue(sourceValue, out sourceTitle);
                if (string.IsNullOrEmpty(sourceTitle))
                {
                    throw new KeyNotFoundException(string.Format("Could not find a DefaultSettings source title for value: '{0}'.", sourceValue));
                }
                else
                {
                    return sourceTitle;
                }
            }
        }

        private async Task<string> GetArTypeAsync(string sourceValue, bool bypassCache)
        {
            if (string.IsNullOrEmpty(sourceValue))
            {
                return string.Empty;
            }

            if (bypassCache)
            {
                var arType = await DataReader.ReadRecordAsync<ArTypesBase>(sourceValue);
                if (arType == null || string.IsNullOrEmpty(arType.ArtDesc))
                {
                    throw new KeyNotFoundException(string.Format("Could not find a DefaultSettings source title for value: '{0}'.", sourceValue));
                }
                return arType.ArtDesc;
            }
            var dictArTypes = await GetAllArTypesAsync(bypassCache);

            if (!dictArTypes.ContainsKey(sourceValue))
            {
                throw new KeyNotFoundException(string.Format("Could not find a DefaultSettings source title for value: '{0}'.", sourceValue));
            }
            else
            {
                string sourceTitle = string.Empty;
                dictArTypes.TryGetValue(sourceValue, out sourceTitle);
                if (string.IsNullOrEmpty(sourceTitle))
                {
                    throw new KeyNotFoundException(string.Format("Could not find a DefaultSettings source title for value: '{0}'.", sourceValue));
                }
                else
                {
                    return sourceTitle;
                }
            }
        }

        private async Task<string> GetApTypeAsync(string sourceValue, bool bypassCache)
        {
            if (string.IsNullOrEmpty(sourceValue))
            {
                return string.Empty;
            }

            if (bypassCache)
            {
                var apType = await DataReader.ReadRecordAsync<ApTypesBase>(sourceValue);
                if (apType == null || string.IsNullOrEmpty(apType.ApTypesDesc))
                {
                    throw new KeyNotFoundException(string.Format("Could not find a DefaultSettings source title for value: '{0}'.", sourceValue));
                }
                return apType.ApTypesDesc;
            }
            var dictApTypes = await GetAllApTypesAsync(bypassCache);

            if (!dictApTypes.ContainsKey(sourceValue))
            {
                throw new KeyNotFoundException(string.Format("Could not find a DefaultSettings source title for value: '{0}'.", sourceValue));
            }
            else
            {
                string sourceTitle = string.Empty;
                dictApTypes.TryGetValue(sourceValue, out sourceTitle);
                if (string.IsNullOrEmpty(sourceTitle))
                {
                    throw new KeyNotFoundException(string.Format("Could not find a DefaultSettings source title for value: '{0}'.", sourceValue));
                }
                else
                {
                    return sourceTitle;
                }
            }
        }

        private async Task<string> GetCreditTypeAsync(string sourceValue, bool bypassCache)
        {
            if (string.IsNullOrEmpty(sourceValue))
            {
                return string.Empty;
            }

            if (bypassCache)
            {
                var credType = await DataReader.ReadRecordAsync<CredTypesBase>(sourceValue);
                if (credType == null || string.IsNullOrEmpty(credType.CrtpDesc))
                {
                    throw new KeyNotFoundException(string.Format("Could not find a DefaultSettings source title for value: '{0}'.", sourceValue));
                }
                return credType.CrtpDesc;
            }
            var dictCreditTypes = await GetAllCreditTypesAsync(bypassCache);

            if (!dictCreditTypes.ContainsKey(sourceValue))
            {
                throw new KeyNotFoundException(string.Format("Could not find a DefaultSettings source title for value: '{0}'.", sourceValue));
            }
            else
            {
                string sourceTitle = string.Empty;
                dictCreditTypes.TryGetValue(sourceValue, out sourceTitle);
                if (string.IsNullOrEmpty(sourceTitle))
                {
                    throw new KeyNotFoundException(string.Format("Could not find a DefaultSettings source title for value: '{0}'.", sourceValue));
                }
                else
                {
                    return sourceTitle;
                }
            }
        }

        private async Task<string> GetAcadLevelAsync(string sourceValue, bool bypassCache)
        {
            if (string.IsNullOrEmpty(sourceValue))
            {
                return string.Empty;
            }

            if (bypassCache)
            {
                var acadLevel = await DataReader.ReadRecordAsync<AcadLevelsBase>(sourceValue);
                if (acadLevel == null || string.IsNullOrEmpty(acadLevel.AclvDesc))
                {
                    throw new KeyNotFoundException(string.Format("Could not find a DefaultSettings source title for value: '{0}'.", sourceValue));
                }
                return acadLevel.AclvDesc;
            }
            var dictAcadLevels = await GetAllAcademicLevelsAsync(bypassCache);

            if (!dictAcadLevels.ContainsKey(sourceValue))
            {
                throw new KeyNotFoundException(string.Format("Could not find a DefaultSettings source title for value: '{0}'.", sourceValue));
            }
            else
            {
                string sourceTitle = string.Empty;
                dictAcadLevels.TryGetValue(sourceValue, out sourceTitle);
                if (string.IsNullOrEmpty(sourceTitle))
                {
                    throw new KeyNotFoundException(string.Format("Could not find a DefaultSettings source title for value: '{0}'.", sourceValue));
                }
                else
                {
                    return sourceTitle;
                }
            }
        }

        private async Task<string> GetCourseLevelAsync(string sourceValue, bool bypassCache)
        {
            if (string.IsNullOrEmpty(sourceValue))
            {
                return string.Empty;
            }

            var dictValcodes = await GetAllValcodeItemsAsync("ST.VALCODES", "COURSE.LEVELS", bypassCache);

            if (!dictValcodes.ContainsKey(sourceValue))
            {
                throw new KeyNotFoundException(string.Format("Could not find a DefaultSettings source title for value: '{0}'.", sourceValue));
            }
            else
            {
                string sourceTitle = string.Empty;
                dictValcodes.TryGetValue(sourceValue, out sourceTitle);
                if (string.IsNullOrEmpty(sourceTitle))
                {
                    throw new KeyNotFoundException(string.Format("Could not find a DefaultSettings source title for value: '{0}'.", sourceValue));
                }
                else
                {
                    return sourceTitle;
                }
            }
        }

        private async Task<string> GetTeachingArrangementAsync(string sourceValue, bool bypassCache)
        {
            if (string.IsNullOrEmpty(sourceValue))
            {
                return string.Empty;
            }

            var dictValcodes = await GetAllValcodeItemsAsync("ST.VALCODES", "TEACHING.ARRANGEMENTS", bypassCache);

            if (!dictValcodes.ContainsKey(sourceValue))
            {
                throw new KeyNotFoundException(string.Format("Could not find a DefaultSettings source title for value: '{0}'.", sourceValue));
            }
            else
            {
                string sourceTitle = string.Empty;
                dictValcodes.TryGetValue(sourceValue, out sourceTitle);
                if (string.IsNullOrEmpty(sourceTitle))
                {
                    throw new KeyNotFoundException(string.Format("Could not find a DefaultSettings source title for value: '{0}'.", sourceValue));
                }
                else
                {
                    return sourceTitle;
                }
            }
        }

        private async Task<string> GetAssignmentContractTypeAsync(string sourceValue, bool bypassCache)
        {
            if (string.IsNullOrEmpty(sourceValue))
            {
                return string.Empty;
            }

            if (bypassCache)
            {
                var assignmentContractType = await DataReader.ReadRecordAsync<AsgmtContractTypesBase>(sourceValue);
                if (assignmentContractType == null || string.IsNullOrEmpty(assignmentContractType.ActypDesc))
                {
                    throw new KeyNotFoundException(string.Format("Could not find a DefaultSettings source title for value: '{0}'.", sourceValue));
                }
                return assignmentContractType.ActypDesc;
            }
            var dictAssignmentContractTypes = await GetAllAssignmentContractTypesAsync(bypassCache);

            if (!dictAssignmentContractTypes.ContainsKey(sourceValue))
            {
                throw new KeyNotFoundException(string.Format("Could not find a DefaultSettings source title for value: '{0}'.", sourceValue));
            }
            else
            {
                string sourceTitle = string.Empty;
                dictAssignmentContractTypes.TryGetValue(sourceValue, out sourceTitle);
                if (string.IsNullOrEmpty(sourceTitle))
                {
                    throw new KeyNotFoundException(string.Format("Could not find a DefaultSettings source title for value: '{0}'.", sourceValue));
                }
                else
                {
                    return sourceTitle;
                }
            }
        }

        private async Task<string> GetPositionAsync(string sourceValue, bool bypassCache)
        {
            if (string.IsNullOrEmpty(sourceValue))
            {
                return string.Empty;
            }

            if (bypassCache)
            {
                var position = await DataReader.ReadRecordAsync<PositionBase>(sourceValue);
                if (position == null || string.IsNullOrEmpty(position.PosTitle))
                {
                    throw new KeyNotFoundException(string.Format("Could not find a DefaultSettings source title for value: '{0}'.", sourceValue));
                }
                return position.PosTitle;
            }
            var dictPositions = await GetAllPositionsAsync(bypassCache);

            if (!dictPositions.ContainsKey(sourceValue))
            {
                throw new KeyNotFoundException(string.Format("Could not find a DefaultSettings source title for value: '{0}'.", sourceValue));
            }
            else
            {
                string sourceTitle = string.Empty;
                dictPositions.TryGetValue(sourceValue, out sourceTitle);
                if (string.IsNullOrEmpty(sourceTitle))
                {
                    throw new KeyNotFoundException(string.Format("Could not find a DefaultSettings source title for value: '{0}'.", sourceValue));
                }
                else
                {
                    return sourceTitle;
                }
            }
        }

        private async Task<string> GetLoadPeriodAsync(string sourceValue, bool bypassCache)
        {
            if (string.IsNullOrEmpty(sourceValue))
            {
                return string.Empty;
            }

            if (bypassCache)
            {
                var loadPeriod = await DataReader.ReadRecordAsync<LoadPeriodsBase>(sourceValue);
                if (loadPeriod == null || string.IsNullOrEmpty(loadPeriod.LdpdDesc))
                {
                    throw new KeyNotFoundException(string.Format("Could not find a DefaultSettings source title for value: '{0}'.", sourceValue));
                }
                return loadPeriod.LdpdDesc;
            }
            var dictLoadPeriods = await GetAllLoadPeriodsAsync(bypassCache);

            if (!dictLoadPeriods.ContainsKey(sourceValue))
            {
                throw new KeyNotFoundException(string.Format("Could not find a DefaultSettings source title for value: '{0}'.", sourceValue));
            }
            else
            {
                string sourceTitle = string.Empty;
                dictLoadPeriods.TryGetValue(sourceValue, out sourceTitle);
                if (string.IsNullOrEmpty(sourceTitle))
                {
                    throw new KeyNotFoundException(string.Format("Could not find a DefaultSettings source title for value: '{0}'.", sourceValue));
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
                throw new KeyNotFoundException(string.Format("Could not find a DefaultSettings source title for value: '{0}'.", sourceValue));
            }
            else
            {
                string sourceTitle = string.Empty;
                dictBendedCodes.TryGetValue(sourceValue, out sourceTitle);
                if (string.IsNullOrEmpty(sourceTitle))
                {
                    throw new KeyNotFoundException(string.Format("Could not find a DefaultSettings source title for value: '{0}'.", sourceValue));
                }
                else
                {
                    return sourceTitle;
                }
            }
        }

        private async Task<string> GetCourseStatusAsync(string sourceValue, bool bypassCache)
        {
            if (string.IsNullOrEmpty(sourceValue))
            {
                return string.Empty;
            }

            var dictValcodes = await GetAllValcodeItemsAsync("ST.VALCODES", "COURSE.STATUSES", bypassCache);

            if (!dictValcodes.ContainsKey(sourceValue))
            {
                throw new KeyNotFoundException(string.Format("Could not find a DefaultSettings source title for value: '{0}'.", sourceValue));
            }
            else
            {
                string sourceTitle = string.Empty;
                dictValcodes.TryGetValue(sourceValue, out sourceTitle);
                if (string.IsNullOrEmpty(sourceTitle))
                {
                    throw new KeyNotFoundException(string.Format("Could not find a DefaultSettings source title for value: '{0}'.", sourceValue));
                }
                else
                {
                    return sourceTitle;
                }
            }
        }

        private async Task<string> GetApplicationStatusAsync(string sourceValue, bool bypassCache)
        {
            if (string.IsNullOrEmpty(sourceValue))
            {
                return string.Empty;
            }

            if (bypassCache)
            {
                var applicationStatus = await DataReader.ReadRecordAsync<ApplicationStatusesBase>(sourceValue);
                if (applicationStatus == null || string.IsNullOrEmpty(applicationStatus.AppsDesc))
                {
                    throw new KeyNotFoundException(string.Format("Could not find a DefaultSettings source title for value: '{0}'.", sourceValue));
                }
                return applicationStatus.AppsDesc;
            }
            var dictApplicationStatuses = await GetAllApplicationStatusesAsync(bypassCache);

            if (!dictApplicationStatuses.ContainsKey(sourceValue))
            {
                throw new KeyNotFoundException(string.Format("Could not find a DefaultSettings source title for value: '{0}'.", sourceValue));
            }
            else
            {
                string sourceTitle = string.Empty;
                dictApplicationStatuses.TryGetValue(sourceValue, out sourceTitle);
                if (string.IsNullOrEmpty(sourceTitle))
                {
                    throw new KeyNotFoundException(string.Format("Could not find a DefaultSettings source title for value: '{0}'.", sourceValue));
                }
                else
                {
                    return sourceTitle;
                }
            }
        }

        private async Task<string> GetReceiptTenderGlDistrAsync(string sourceValue, bool bypassCache)
        {
            if (string.IsNullOrEmpty(sourceValue))
            {
                return string.Empty;
            }

            if (bypassCache)
            {
                var receiptTenderGlDistr = await DataReader.ReadRecordAsync<RcptTenderGlDistrBase>(sourceValue);
                if (receiptTenderGlDistr == null || string.IsNullOrEmpty(receiptTenderGlDistr.RcpttDesc))
                {
                    throw new KeyNotFoundException(string.Format("Could not find a DefaultSettings source title for value: '{0}'.", sourceValue));
                }
                return receiptTenderGlDistr.RcpttDesc;
            }
            var dictRcptTenderGlDistrs = await GetAllReceiptTenderGlDistrsAsync(bypassCache);

            if (!dictRcptTenderGlDistrs.ContainsKey(sourceValue))
            {
                throw new KeyNotFoundException(string.Format("Could not find a DefaultSettings source title for value: '{0}'.", sourceValue));
            }
            else
            {
                string sourceTitle = string.Empty;
                dictRcptTenderGlDistrs.TryGetValue(sourceValue, out sourceTitle);
                if (string.IsNullOrEmpty(sourceTitle))
                {
                    throw new KeyNotFoundException(string.Format("Could not find a DefaultSettings source title for value: '{0}'.", sourceValue));
                }
                else
                {
                    return sourceTitle;
                }
            }
        }

        private async Task<string> GetPaymentMethodAsync(string sourceValue, bool bypassCache)
        {
            if (string.IsNullOrEmpty(sourceValue))
            {
                return string.Empty;
            }

            if (bypassCache)
            {
                var paymentMethod = await DataReader.ReadRecordAsync<PaymentMethods>(sourceValue);
                if (paymentMethod == null || string.IsNullOrEmpty(paymentMethod.PmthDescription))
                {
                    throw new KeyNotFoundException(string.Format("Could not find a DefaultSettings source title for value: '{0}'.", sourceValue));
                }
                return paymentMethod.PmthDescription;
            }
            var dictPaymentMethods = await GetAllPaymentMethodsAsync(bypassCache);

            if (!dictPaymentMethods.ContainsKey(sourceValue))
            {
                throw new KeyNotFoundException(string.Format("Could not find a DefaultSettings source title for value: '{0}'.", sourceValue));
            }
            else
            {
                string sourceTitle = string.Empty;
                dictPaymentMethods.TryGetValue(sourceValue, out sourceTitle);
                if (string.IsNullOrEmpty(sourceTitle))
                {
                    throw new KeyNotFoundException(string.Format("Could not find a DefaultSettings source title for value: '{0}'.", sourceValue));
                }
                else
                {
                    return sourceTitle;
                }
            }
        }

        private async Task<string> GetPersonNameAsync(string sourceValue, bool bypassCache)
        {
            if (string.IsNullOrEmpty(sourceValue))
            {
                return string.Empty;
            }
            var person = await DataReader.ReadRecordAsync<DataContracts.Person>("PERSON", sourceValue);
            if (person == null || string.IsNullOrEmpty(person.LastName))
            {
                throw new KeyNotFoundException(string.Format("Could not find a DefaultSettings source title for value: '{0}'.", sourceValue));
            }
            else
            {
                if (!string.IsNullOrEmpty(person.PersonCorpIndicator) && person.PersonCorpIndicator.ToUpperInvariant() == "Y")
                {
                    return string.IsNullOrEmpty(person.PreferredName) ? person.LastName : person.PreferredName;
                }
                return string.Concat(person.FirstName, " ", person.LastName);
            }
        }

        private async Task<string> GetPrivacyCodeAsync(string sourceValue, bool bypassCache)
        {
            if (string.IsNullOrEmpty(sourceValue))
            {
                return string.Empty;
            }

            var dictValcodes = await GetAllValcodeItemsAsync("CORE.VALCODES", "PRIVACY.CODES", bypassCache);

            if (!dictValcodes.ContainsKey(sourceValue))
            {
                throw new KeyNotFoundException(string.Format("Could not find a DefaultSettings source title for value: '{0}'.", sourceValue));
            }
            else
            {
                string sourceTitle = string.Empty;
                dictValcodes.TryGetValue(sourceValue, out sourceTitle);
                if (string.IsNullOrEmpty(sourceTitle))
                {
                    throw new KeyNotFoundException(string.Format("Could not find a DefaultSettings source title for value: '{0}'.", sourceValue));
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