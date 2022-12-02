/*Copyright 2019-2021 Ellucian Company L.P. and its affiliates. */

using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Services;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Data.Base.Transactions;
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
using System.Text;

namespace Ellucian.Colleague.Data.Base.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class MappingSettingsRepository : BaseColleagueRepository, IMappingSettingsRepository
    {
        private RepositoryException exception = new RepositoryException();
        //public static char _VM = Convert.ToChar(DynamicArray.VM);
        private readonly int _readSize;
        const int AllIntgMappingSettingsCacheTimeout = 20; // Clear from cache every 20 minutes
        const string AllIntgMappingSettingsCache = "AllIntgMappingSettings";
        const int AllIntgMappingSettingsOptionsCacheTimeout = 20; // Clear from cache every 20 minutes
        const string AllIntgMappingSettingsOptionsCache = "AllIntgMappingSettingsOptions";


        public MappingSettingsRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            CacheTimeout = Level1CacheTimeoutValue;
            this._readSize = ((apiSettings != null) && (apiSettings.BulkReadSize > 0)) ? apiSettings.BulkReadSize : 5000;
        }

        #region GET mapping-settings
        /// <summary>
        /// Get a collection of IntgMappingSettings
        /// </summary>
        /// <param name="bypassCache">Bypass cache flag</param>
        /// <returns>Collection of IntgMappingSettings</returns>
        public async Task<Tuple<IEnumerable<MappingSettings>, int>> GetMappingSettingsAsync(int offset, int limit, List<string> resources, List<string> propertyNames, bool bypassCache)
        {            
            string[] intgMappingSettingsIds = new string[] { };
            int totalCount = 0;
            string[] subList = null;

            string intgMappingSettingsCacheKey = CacheSupport.BuildCacheKey(AllIntgMappingSettingsCache, resources, propertyNames);
            if (bypassCache && ContainsKey(BuildFullCacheKey(intgMappingSettingsCacheKey)))
            {
                ClearCache(new List<string> { intgMappingSettingsCacheKey });
            }
            var keyCache = await CacheSupport.GetOrAddKeyCacheToCache(
                this,
                ContainsKey,
                GetOrAddToCacheAsync,
                AddOrUpdateCacheAsync,
                transactionInvoker,
                intgMappingSettingsCacheKey,
                "INTG.MAPPING.SETTINGS",
                offset,
                limit,
                AllIntgMappingSettingsCacheTimeout,
                async () =>
                {
                    var criteria = new StringBuilder();
                    if (resources != null && resources.Any())
                    {
                        foreach (var resource in resources)
                        {
                            var thisResourceString = string.Format("WITH IMS.ETHOS.RESOURCE = '{0}'", resource);
                            criteria.AppendFormat(thisResourceString);
                        }
                    }

                    if (propertyNames != null && propertyNames.Any())
                    {
                        var mappingInfoCriteria = new StringBuilder();
                        foreach (var propertyName in propertyNames)
                        {
                            var thisPropertyNameString = string.Format("WITH IMN.ETHOS.PROPERTY.NAME = '{0}'", propertyName);
                            mappingInfoCriteria.AppendFormat(thisPropertyNameString);
                        }
                        var limitingKeys = await DataReader.SelectAsync("INTG.MAPPING.INFO", mappingInfoCriteria.ToString());

                        if (limitingKeys == null || !limitingKeys.Any())
                        {
                            return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                        }

                        var propertyNameString = string.Empty;
                        foreach (var limitingKey in limitingKeys)
                        {
                            if (string.IsNullOrEmpty(propertyNameString))
                            {
                                propertyNameString = string.Format("WITH IMS.MAPPING.INFO.ID EQ '{0}'", limitingKey);
                            }
                            else
                            {
                                propertyNameString += string.Format(" '{0}'", limitingKey);
                            }
                        }
                        criteria.AppendFormat(propertyNameString);
                    }

                    CacheSupport.KeyCacheRequirements requirements = new CacheSupport.KeyCacheRequirements()
                    {
                        criteria = criteria.ToString(),
                    };
                    return requirements;
                }
            );

            if (keyCache == null || keyCache.Sublist == null || !keyCache.Sublist.Any())
            {
                return new Tuple<IEnumerable<MappingSettings>, int>(new List<MappingSettings>(), 0);
            }

            subList = keyCache.Sublist.ToArray();

            totalCount = keyCache.TotalCount.Value;

            var mappingSettingsData = await DataReader.BulkReadRecordAsync<IntgMappingSettings>("INTG.MAPPING.SETTINGS", subList);
            //
            // Extract first part of INTG.MAPPING.SETTINGS key for INTG.MAPPING.INFO keys
            //
            List<string> allIntgMappingInfoIds = subList.Select(x => x.Split('*')[0]).Where(y => !string.IsNullOrEmpty(y)).ToList();
            var intgMappingInfoIds = allIntgMappingInfoIds.Distinct().ToList().ToArray();
            var mappingSettingsInfo = await DataReader.BulkReadRecordAsync<IntgMappingInfo>("INTG.MAPPING.INFO", intgMappingInfoIds);

            var mappingSettingsCollection = await BuildMappingSettingsAsync(mappingSettingsData, mappingSettingsInfo, bypassCache);

            return new Tuple<IEnumerable<MappingSettings>, int>(mappingSettingsCollection, totalCount);
        }

        /// <summary>
        /// Get a single Mapping Settings domain entity 
        /// </summary>
        /// <param name="guid">Guid for Lookup</param>
        /// <param name="bypassCache"> Bypass Cache flag</param>
        /// <returns></returns>
        public async Task<MappingSettings> GetMappingSettingsByGuidAsync(string guid, bool bypassCache = false)
        {
            return await GetMappingSettingsByIdAsync(await GetMappingSettingsIdFromGuidAsync(guid), bypassCache);
        }

        /// <summary>
        /// Get the record key from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>Primary key</returns>
        public async Task<string> GetMappingSettingsIdFromGuidAsync(string guid)
        {

            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }
            var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(guid) });
            if (idDict == null || idDict.Count == 0)
            {
                throw new KeyNotFoundException("INTG.MAPPING.SETTINGS GUID " + guid + " not found.");
            }
            var foundEntry = idDict.FirstOrDefault();
            if (foundEntry.Value == null || foundEntry.Value.Entity != "INTG.MAPPING.SETTINGS" || !string.IsNullOrEmpty(foundEntry.Value.SecondaryKey))
            {
                throw new KeyNotFoundException(string.Format("The GUID specified: '{0}' is used by a different entity/secondary key: {1}", guid, foundEntry.Value != null ? foundEntry.Value.Entity : string.Empty));
            }
            return foundEntry.Value.PrimaryKey;
        }

        /// <summary>
        /// Get a single Mapping Settings domain entity
        /// </summary>
        /// <param name="id">The MappingSettings id</param>
        /// <param name="bypassCache">Bypass Cache flag</param>
        /// <returns>Mapping Settings domain entity object</returns>
        private async Task<MappingSettings> GetMappingSettingsByIdAsync(string id, bool bypassCache)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID is required to get a MappingSetting.");
            }

            var mappingSettingsData = await DataReader.ReadRecordAsync<IntgMappingSettings>(id);
            if (mappingSettingsData == null)
            {
                throw new KeyNotFoundException(string.Concat("Record not found, or MappingSetting with ID ", id, "invalid."));
            }

            var mappingInfoId = id.Split('*')[0];
            var mappingInfoData = await DataReader.ReadRecordAsync<IntgMappingInfo>("INTG.MAPPING.INFO", mappingInfoId);
            if (mappingInfoData == null)
            {
                exception.AddError(new RepositoryError("Bad.Data", "Could not read INTG.MAPPING.INFO record " + mappingInfoId + "."));
                throw exception;
            }
            var mappingSetting = await BuildMappingSettingAsync(mappingSettingsData, mappingInfoData, bypassCache);

            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }

            return mappingSetting;
        }

        #region Build MappingSettings

        /// <summary>
        /// Build from a Collection of Data Contracts
        /// </summary>
        /// <param name="sources">INTG.MAPPING.SETTINGS data contracts</param>
        /// <param name="intgMappingInfos">INTG.MAPPING.INFO data contracts</param>
        /// <param name="bypassCache">bypass chache flag</param>
        /// <returns></returns>
        private async Task<IEnumerable<MappingSettings>> BuildMappingSettingsAsync(Collection<IntgMappingSettings> sources, Collection<IntgMappingInfo> intgMappingInfos, bool bypassCache = false)
        {
            var mappingSettingsCollection = new List<MappingSettings>();
            foreach (var source in sources)
            {
                // Extract the one INTG.MAPPING.INFO data contract for the one INTG.MAPPING.SETTING data contract
                var matchingIntgMappingInfo = intgMappingInfos.Where(im => (im.Recordkey == source.Recordkey.Split('*')[0]));
                foreach (var intgMappingInfo in matchingIntgMappingInfo)
                {
                    mappingSettingsCollection.Add(await BuildMappingSettingAsync(source, intgMappingInfo, bypassCache));
                }
            }
            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }
            return mappingSettingsCollection.AsEnumerable();
        }

        /// <summary>
        /// Build a single mapping-settings entry
        /// </summary>
        /// <param name="source">INTG.MAPPING.SETTINGS data contract</param>
        /// <param name="intgMappingInfo">INTG.MAPPING.INFO data contract</param>
        /// <param name="bypassCache">bypass cache flag</param>
        /// <returns></returns>
        private async Task<MappingSettings> BuildMappingSettingAsync(IntgMappingSettings source, IntgMappingInfo intgMappingInfo, bool bypassCache = false)
        {
            if (source == null)
            {
                exception.AddError(new RepositoryError("Bad.Data", "Selected, but can not read INTG.MAPPING.SETTINGS record."));
                return null;
            }

            MappingSettings mappingSetting = null;
            try
            {
                var sourceValue = source.Recordkey.Split('*')[1];
                var sourceTitle = string.Empty;
                var mappedValue = string.Empty;
                //
                //  Get the title (description) of the source value (code).
                //  And if populated, get the Ethos mapping enumeration.
                //  Note: Ethos mappings for valcodes store the Ethos enumeration itself
                //        But mappings for code files store a code that needs to be translated to an Ethos enumeration
                //        using the INTG... valcode that stores the possible enumerations
                //
                if (sourceValue != null)
                {
                    switch (source.ImsCollEntity.ToUpperInvariant())
                    {
                        //
                        // Get source title (description) and mapped value from valcode sources.
                        // valcodeInfo[0] = code description
                        // valcodeInfo[1] = mapped value (Ethos enumeration)
                        //
                        case "CORE.VALCODES":
                            if (!string.IsNullOrEmpty(source.ImsCollValcodeId))
                            {
                                if (source.ImsCollValcodeId.ToUpperInvariant() == "PERSON.EMAIL.TYPES")
                                {
                                    var valcodeInfo = await GetEmailTypesAsync(sourceValue, bypassCache);
                                    if (!string.IsNullOrEmpty(valcodeInfo[0]))
                                    {
                                        sourceTitle = valcodeInfo[0];
                                    }
                                    if (!string.IsNullOrEmpty(valcodeInfo[1]))
                                    {
                                        mappedValue = valcodeInfo[1];
                                    }
                                    break;
                                }
                                else if (source.ImsCollValcodeId.ToUpperInvariant() == "PHONE.TYPES")
                                {
                                    var valcodeInfo = await GetPhoneTypesAsync(sourceValue, bypassCache);
                                    if (!string.IsNullOrEmpty(valcodeInfo[0]))
                                    {
                                        sourceTitle = valcodeInfo[0];
                                    }
                                    if (!string.IsNullOrEmpty(valcodeInfo[1]))
                                    {
                                        mappedValue = valcodeInfo[1];
                                    }
                                    break;
                                }
                                else if (source.ImsCollValcodeId.ToUpperInvariant() == "ADREL.TYPES")
                                {
                                    var valcodeInfo = await GetAdrelTypesAsync(sourceValue, bypassCache);
                                    if (!string.IsNullOrEmpty(valcodeInfo[0]))
                                    {
                                        sourceTitle = valcodeInfo[0];
                                    }
                                    if (!string.IsNullOrEmpty(valcodeInfo[1]))
                                    {
                                        mappedValue = valcodeInfo[1];
                                    }
                                    break;
                                }
                                else if (source.ImsCollValcodeId.ToUpperInvariant() == "MIL.STATUSES")
                                {
                                    var valcodeInfo = await GetMilStatusesAsync(sourceValue, bypassCache);
                                    if (!string.IsNullOrEmpty(valcodeInfo[0]))
                                    {
                                        sourceTitle = valcodeInfo[0];
                                    }
                                    if (!string.IsNullOrEmpty(valcodeInfo[1]))
                                    {
                                        mappedValue = valcodeInfo[1];
                                    }
                                    break;
                                }
                                else if (source.ImsCollValcodeId.ToUpperInvariant() == "SOCIAL.MEDIA.NETWORKS")
                                {
                                    var valcodeInfo = await GetSocialMediaNetworks(sourceValue, bypassCache);
                                    if (!string.IsNullOrEmpty(valcodeInfo[0]))
                                    {
                                        sourceTitle = valcodeInfo[0];
                                    }
                                    if (!string.IsNullOrEmpty(valcodeInfo[1]))
                                    {
                                        mappedValue = valcodeInfo[1];
                                    }
                                    break;
                                }
                                else if (source.ImsCollValcodeId.ToUpperInvariant() == "VISA.TYPES")
                                {
                                    var valcodeInfo = await GetVisaTypesAsync(sourceValue, bypassCache);
                                    if (!string.IsNullOrEmpty(valcodeInfo[0]))
                                    {
                                        sourceTitle = valcodeInfo[0];
                                    }
                                    if (!string.IsNullOrEmpty(valcodeInfo[1]))
                                    {
                                        mappedValue = valcodeInfo[1];
                                    }
                                    break;
                                }
                                else if (source.ImsCollValcodeId.ToUpperInvariant() == "ROOM.TYPES")
                                {
                                    var valcodeInfo = await GetRoomTypesAsync(sourceValue, bypassCache);
                                    if (!string.IsNullOrEmpty(valcodeInfo[0]))
                                    {
                                        sourceTitle = valcodeInfo[0];
                                    }
                                    if (!string.IsNullOrEmpty(valcodeInfo[1]))
                                    {
                                        mappedValue = valcodeInfo[1];
                                    }
                                    break;
                                }
                                else
                                {
                                    exception.AddError(new RepositoryError("Missing.Required.Property", "Invalid CORE Colleague valcode " + source.ImsCollValcodeId)
                                    {
                                        Id = source.RecordGuid,
                                        SourceId = source.Recordkey
                                    });
                                    break;
                                }
                            }
                            else
                            {
                                exception.AddError(new RepositoryError("Missing.Required.Property", "Missing source CORE Colleague valcode")
                                {
                                    Id = source.RecordGuid,
                                    SourceId = source.Recordkey
                                });
                                break;
                            }
                        case "ST.VALCODES":
                            if (!string.IsNullOrEmpty(source.ImsCollValcodeId))
                            {
                                if (source.ImsCollValcodeId.ToUpperInvariant() == "SECTION.STATUSES")
                                {
                                    var valcodeInfo = await GetSectionStatusesAsync(sourceValue, bypassCache);
                                    if (!string.IsNullOrEmpty(valcodeInfo[0]))
                                    {
                                        sourceTitle = valcodeInfo[0];
                                    }
                                    if (!string.IsNullOrEmpty(valcodeInfo[1]))
                                    {
                                        mappedValue = valcodeInfo[1];
                                    }
                                    break;
                                }
                                else if (source.ImsCollValcodeId.ToUpperInvariant() == "CONTACT.MEASURES")
                                {
                                    var valcodeInfo = await GetContactMeasuresAsync(sourceValue, bypassCache);
                                    if (!string.IsNullOrEmpty(valcodeInfo[0]))
                                    {
                                        sourceTitle = valcodeInfo[0];
                                    }
                                    if (!string.IsNullOrEmpty(valcodeInfo[1]))
                                    {
                                        mappedValue = valcodeInfo[1];
                                    }
                                    break;
                                }
                                else if (source.ImsCollValcodeId.ToUpperInvariant() == "ROOM.ASSIGN.STATUSES")
                                {
                                    var valcodeInfo = await GetRoomAssignStatusesAsync(sourceValue, bypassCache);
                                    if (!string.IsNullOrEmpty(valcodeInfo[0]))
                                    {
                                        sourceTitle = valcodeInfo[0];
                                    }
                                    if (!string.IsNullOrEmpty(valcodeInfo[1]))
                                    {
                                        if (source.ImsEthosResource == "housing-assignments")
                                        {
                                            // housing-assignments uses VAL.ACTION.CODE.3
                                            mappedValue = valcodeInfo[1];
                                        }
                                    }
                                    if (!string.IsNullOrEmpty(valcodeInfo[2]))
                                    {
                                        if (source.ImsEthosResource == "housing-requests")
                                        {
                                            // housing-requests uses VAL.ACTION.CODE.4
                                            mappedValue = valcodeInfo[2];
                                        }
                                    }
                                    break;
                                }
                                else if (source.ImsCollValcodeId.ToUpperInvariant() == "MEAL.ASSIGN.STATUSES")
                                {
                                    var valcodeInfo = await GetMealAssignStatusesAsync(sourceValue, bypassCache);
                                    if (!string.IsNullOrEmpty(valcodeInfo[0]))
                                    {
                                        sourceTitle = valcodeInfo[0];
                                    }
                                    if (!string.IsNullOrEmpty(valcodeInfo[1]))
                                    {
                                        mappedValue = valcodeInfo[1];
                                    }
                                    break;
                                }
                                else
                                {
                                    exception.AddError(new RepositoryError("Missing.Required.Property", "Invalid ST Colleague valcode " + source.ImsCollValcodeId)
                                    {
                                        Id = source.RecordGuid,
                                        SourceId = source.Recordkey
                                    });
                                    break;
                                }
                            }
                            else
                            {
                                exception.AddError(new RepositoryError("Missing.Required.Property", "Missing source ST Colleague valcode")
                                {
                                    Id = source.RecordGuid,
                                    SourceId = source.Recordkey
                                });
                                break;
                            }
                        case "HR.VALCODES":
                            if (!string.IsNullOrEmpty(source.ImsCollValcodeId))
                            {
                                if (source.ImsCollValcodeId.ToUpperInvariant() == "REHIRE.ELIGIBILITY.CODES")
                                {
                                    var valcodeInfo = await GetRehireEligibilityCodesAsync(sourceValue, bypassCache);
                                    if (!string.IsNullOrEmpty(valcodeInfo[0]))
                                    {
                                        sourceTitle = valcodeInfo[0];
                                    }
                                    if (!string.IsNullOrEmpty(valcodeInfo[1]))
                                    {
                                        mappedValue = valcodeInfo[1];
                                    }
                                    break;
                                }
                                else if (source.ImsCollValcodeId.ToUpperInvariant() == "HR.STATUSES")
                                {
                                    var valcodeInfo = await GetHrStatusesAsync(sourceValue, bypassCache);
                                    if (!string.IsNullOrEmpty(valcodeInfo[0]))
                                    {
                                        sourceTitle = valcodeInfo[0];
                                    }
                                    if (!string.IsNullOrEmpty(valcodeInfo[1]))
                                    {
                                        mappedValue = valcodeInfo[1];
                                    }
                                    break;
                                }
                                else
                                {
                                    exception.AddError(new RepositoryError("Missing.Required.Property", "Invalid HR Colleague valcode " + source.ImsCollValcodeId)
                                    {
                                        Id = source.RecordGuid,
                                        SourceId = source.Recordkey
                                    });
                                    break;
                                }
                            }
                            else
                            {
                                exception.AddError(new RepositoryError("Missing.Required.Property", "Missing source HR Colleague valcode")
                                {
                                    Id = source.RecordGuid,
                                    SourceId = source.Recordkey
                                });
                                break;
                            }

                        //
                        // Get source title (description) and mapped value from code files
                        // <code file>Info[0] = code description
                        // <code file>Info[1] = mapped value that needs translation to Ethos enumeration
                        //
                        case "RESTRICTIONS":
                            var restrictionInfo = await GetRestrictionAsync(sourceValue, bypassCache);
                            if (restrictionInfo != null)
                            {
                                if (!string.IsNullOrEmpty(restrictionInfo[0]))
                                {
                                    sourceTitle = restrictionInfo[0];
                                }
                                if (!string.IsNullOrEmpty(restrictionInfo[1]))
                                {
                                    // translate mapped value to Ethos enumeration from Ethos valcode
                                    var valcodeInfo = await GetIntgRestrCategoriesAsync(restrictionInfo[1], bypassCache);
                                    if (!string.IsNullOrEmpty(valcodeInfo[0]))
                                    {
                                        mappedValue = valcodeInfo[0];
                                    }
                                }
                            }
                            break;
                        case "RELATION.TYPES":
                            //
                            // person-relation-types can be mapped from 3 different source fields.
                            // So need to check specific source field to know which mapped value to translate.
                            // relationTypesInfo[0] = code description
                            // relationTypesInfo[1] = mapped value in RELTY.INTG.PERSON.REL.TYPE that needs translation to Ethos enumeration
                            // relationTypesInfo[2] = mapped value in RELTY.INTG.MALE.REL.TYPE that needs translation to Ethos enumeration
                            // relationTypesInfo[3] = mapped value in RELTY.INTG.FEMALE.REL.TYPE that needs translation to Ethos enumeration
                            //
                            var codeFileInfo = await GetRelationTypeAsync(sourceValue, bypassCache);
                            if (codeFileInfo != null)
                            {
                                if (!string.IsNullOrEmpty(codeFileInfo[0]))
                                {
                                    sourceTitle = codeFileInfo[0];
                                }
                                if (!string.IsNullOrEmpty(intgMappingInfo.ImnCollFieldName))
                                {
                                    if (intgMappingInfo.ImnCollFieldName.ToUpperInvariant() == "RELTY.INTG.PERSON.REL.TYPE")
                                    {
                                        if (!string.IsNullOrEmpty(codeFileInfo[1]))
                                        {
                                            // translate mapped value to Ethos enumeration from Ethos valcode
                                            var valcodeInfo = await GetIntgPersonRelationTypesAsync(codeFileInfo[1], bypassCache);
                                            if (!string.IsNullOrEmpty(valcodeInfo[0]))
                                            {
                                                mappedValue = valcodeInfo[0];
                                            }
                                        }
                                    }
                                    if (intgMappingInfo.ImnCollFieldName.ToUpperInvariant() == "RELTY.INTG.MALE.REL.TYPE")
                                    {
                                        if (!string.IsNullOrEmpty(codeFileInfo[2]))
                                        {
                                            // translate mapped value to Ethos enumeration from Ethos valcode
                                            var valcodeInfo = await GetIntgPersonRelationTypesAsync(codeFileInfo[2], bypassCache);
                                            if (!string.IsNullOrEmpty(valcodeInfo[0]))
                                            {
                                                mappedValue = valcodeInfo[0];
                                            }
                                        }
                                    }
                                    if (intgMappingInfo.ImnCollFieldName.ToUpperInvariant() == "RELTY.INTG.FEMALE.REL.TYPE")
                                    {
                                        if (!string.IsNullOrEmpty(codeFileInfo[3]))
                                        {
                                            // translate mapped value to Ethos enumeration from Ethos valcode
                                            var valcodeInfo = await GetIntgPersonRelationTypesAsync(codeFileInfo[3], bypassCache);
                                            if (!string.IsNullOrEmpty(valcodeInfo[0]))
                                            {
                                                mappedValue = valcodeInfo[0];
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        case "ROOM.TYPES":
                            var roomTypeInfo = await GetRoomTypeAsync(sourceValue, bypassCache);
                            if (roomTypeInfo != null)
                            {
                                if (!string.IsNullOrEmpty(roomTypeInfo[0]))
                                {
                                    sourceTitle = roomTypeInfo[0];
                                }
                                if (!string.IsNullOrEmpty(roomTypeInfo[1]))
                                {
                                    var valcodeInfo = await GetIntgRoomTypesAsync(roomTypeInfo[1], bypassCache);
                                    if (!string.IsNullOrEmpty(valcodeInfo[0]))
                                    {
                                        mappedValue = valcodeInfo[0];
                                    }
                                }
                            }
                            break;
                        case "SESSIONS":
                            var sessionInfo = await GetSessionAsync(sourceValue, bypassCache);
                            if (sessionInfo != null)
                            {
                                if (!string.IsNullOrEmpty(sessionInfo[0]))
                                {
                                    sourceTitle = sessionInfo[0];
                                }
                                if (!string.IsNullOrEmpty(sessionInfo[1]))
                                {
                                    var valcodeInfo = await GetIntgSessCategoriesAsync(sessionInfo[1], bypassCache);
                                    if (!string.IsNullOrEmpty(valcodeInfo[0]))
                                    {
                                        mappedValue = valcodeInfo[0];
                                    }
                                }
                            }
                            break;
                        case "AWARD.CATEGORIES":
                            var awardCategoryInfo = await GetAwardCategoryAsync(sourceValue, bypassCache);
                            if (awardCategoryInfo != null)
                            {
                                if (!string.IsNullOrEmpty(awardCategoryInfo[0]))
                                {
                                    sourceTitle = awardCategoryInfo[0];
                                }
                                if (!string.IsNullOrEmpty(awardCategoryInfo[1]))
                                {
                                    var valcodeInfo = await GetIntgAwardCategoryNamesAsync(awardCategoryInfo[1], bypassCache);
                                    if (!string.IsNullOrEmpty(valcodeInfo[0]))
                                    {
                                        mappedValue = valcodeInfo[0];
                                    }
                                }
                            }
                            break;
                        default:
                            exception.AddError(new RepositoryError("Invalid.Colleague.Entity", "Invalid source Colleague Entity '" + source.ImsCollEntity + "'.")
                            {
                                Id = source.RecordGuid,
                                SourceId = source.Recordkey
                            });
                            break;
                    }
                }
                else
                {
                    exception.AddError(new RepositoryError("Missing.Mapping.Info.Key", "Unable to extract mapping info key from mapping setting.")
                    {
                        Id = source.RecordGuid,
                        SourceId = source.Recordkey
                    });
                }

                mappingSetting = new MappingSettings(source.RecordGuid, source.Recordkey, intgMappingInfo.ImnCollMappingTitle)
                {
                    // Add entity fields from each data contract
                    EthosResource = intgMappingInfo.ImnEthosResource,
                    EthosPropertyName = intgMappingInfo.ImnEthosPropertyName,
                    SourceTitle = sourceTitle,
                    SourceValue = sourceValue,
                    Enumeration = mappedValue
                };
            }
            catch (Exception ex)
            {
                exception.AddError(new RepositoryError("Missing.Required.Property", "Unable to build mapping setting.  " + ex.Message)
                {
                    Id = source.RecordGuid,
                    SourceId = source.Recordkey
                });
            }
            return mappingSetting;
        }
        #endregion Build MappingSettings
        #endregion GET mapping-settings

        #region GET mapping-settings-options
        /// <summary>
        /// Get a collection of IntgMappingSettings
        /// </summary>
        /// <param name="bypassCache">Bypass cache flag</param>
        /// <returns>Collection of IntgMappingSettings</returns>
        public async Task<Tuple<IEnumerable<MappingSettingsOptions>, int>> GetMappingSettingsOptionsAsync(int offset, int limit, List<string> resources, List<string> propertyNames, bool bypassCache)
        {
            string[] intgMappingSettingsIds = new string[] { };
            int totalCount = 0;
            string[] subList = null;

            string intgMappingSettingsOptionsCacheKey = CacheSupport.BuildCacheKey(AllIntgMappingSettingsOptionsCache, resources, propertyNames);
            if (bypassCache && ContainsKey(BuildFullCacheKey(intgMappingSettingsOptionsCacheKey)))
            {
                ClearCache(new List<string> { intgMappingSettingsOptionsCacheKey });
            }
            var keyCache = await CacheSupport.GetOrAddKeyCacheToCache(
                this,
                ContainsKey,
                GetOrAddToCacheAsync,
                AddOrUpdateCacheAsync,
                transactionInvoker,
                intgMappingSettingsOptionsCacheKey,
                "INTG.MAPPING.SETTINGS",
                offset,
                limit,
                AllIntgMappingSettingsOptionsCacheTimeout,
                async () =>
                {
                    var criteria = new StringBuilder();
                    if (resources != null && resources.Any())
                    {
                        foreach (var resource in resources)
                        {
                            var thisResourceString = string.Format("WITH IMS.ETHOS.RESOURCE = '{0}'", resource);
                            criteria.AppendFormat(thisResourceString);
                        }
                    }

                    if (propertyNames != null && propertyNames.Any())
                    {
                        var mappingInfoCriteria = new StringBuilder();
                        foreach (var propertyName in propertyNames)
                        {
                            var thisPropertyNameString = string.Format("WITH IMN.ETHOS.PROPERTY.NAME = '{0}'", propertyName);
                            mappingInfoCriteria.AppendFormat(thisPropertyNameString);
                        }
                        var limitingKeys = await DataReader.SelectAsync("INTG.MAPPING.INFO", mappingInfoCriteria.ToString());

                        if (limitingKeys == null || !limitingKeys.Any())
                        {
                            return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                        }

                        var propertyNameString = string.Empty;
                        foreach (var limitingKey in limitingKeys)
                        {
                            if (string.IsNullOrEmpty(propertyNameString))
                            {
                                propertyNameString = string.Format("WITH IMS.MAPPING.INFO.ID EQ '{0}'", limitingKey);
                            }
                            else
                            {
                                propertyNameString += string.Format(" '{0}'", limitingKey);
                            }
                        }
                        criteria.AppendFormat(propertyNameString);
                    }

                    CacheSupport.KeyCacheRequirements requirements = new CacheSupport.KeyCacheRequirements()
                    {
                        criteria = criteria.ToString(),
                    };
                    return requirements;
                }
            );

            if (keyCache == null || keyCache.Sublist == null || !keyCache.Sublist.Any())
            {
                return new Tuple<IEnumerable<MappingSettingsOptions>, int>(new List<MappingSettingsOptions>(), 0);
            }

            subList = keyCache.Sublist.ToArray();

            totalCount = keyCache.TotalCount.Value;

            var mappingSettingsData = await DataReader.BulkReadRecordAsync<IntgMappingSettings>("INTG.MAPPING.SETTINGS", subList);
            //
            // Extract first part of INTG.MAPPING.SETTINGS key for INTG.MAPPING.INFO keys
            //
            List<string> allIntgMappingInfoIds = subList.Select(x => x.Split('*')[0]).Where(y => !string.IsNullOrEmpty(y)).ToList();
            var intgMappingInfoIds = allIntgMappingInfoIds.Distinct().ToList().ToArray();
            var mappingSettingsInfo = await DataReader.BulkReadRecordAsync<IntgMappingInfo>("INTG.MAPPING.INFO", intgMappingInfoIds);

            var mappingSettingsOptionsCollection = await BuildMappingSettingsOptionsAsync(mappingSettingsData, mappingSettingsInfo, bypassCache);

            return new Tuple<IEnumerable<MappingSettingsOptions>, int>(mappingSettingsOptionsCollection, totalCount);
        }

        /// <summary>
        /// Get a single Mapping Settings domain entity 
        /// </summary>
        /// <param name="guid">Guid for Lookup</param>
        /// <param name="bypassCache"> Bypass Cache flag</param>
        /// <returns></returns>
        public async Task<MappingSettingsOptions> GetMappingSettingsOptionsByGuidAsync(string guid, bool bypassCache = false)
        {
            return await GetMappingSettingsOptionsByIdAsync(await GetMappingSettingsOptionsIdFromGuidAsync(guid), bypassCache);
        }

        /// <summary>
        /// Get the record key from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>Primary key</returns>
        public async Task<string> GetMappingSettingsOptionsIdFromGuidAsync(string guid)
        {

            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }
            var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(guid) });
            if (idDict == null || idDict.Count == 0)
            {
                throw new KeyNotFoundException("INTG.MAPPING.SETTINGS GUID " + guid + " not found.");
            }
            var foundEntry = idDict.FirstOrDefault();
            if (foundEntry.Value == null || foundEntry.Value.Entity != "INTG.MAPPING.SETTINGS" || !string.IsNullOrEmpty(foundEntry.Value.SecondaryKey))
            {
                throw new KeyNotFoundException(string.Format("The GUID specified: '{0}' is used by a different entity/secondary key: {1}", guid, foundEntry.Value != null ? foundEntry.Value.Entity : string.Empty));
            }
            return foundEntry.Value.PrimaryKey;
        }

        /// <summary>
        /// Get a single Mapping Settings domain entity
        /// </summary>
        /// <param name="id">The MappingSettings id</param>
        /// <param name="bypassCache">Bypass Cache flag</param>
        /// <returns>Mapping Settings domain entity object</returns>
        private async Task<MappingSettingsOptions> GetMappingSettingsOptionsByIdAsync(string id, bool bypassCache)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID is required to get a MappingSetting.");
            }

            var mappingSettingsData = await DataReader.ReadRecordAsync<IntgMappingSettings>(id);
            if (mappingSettingsData == null)
            {
                throw new KeyNotFoundException(string.Concat("Record not found, or MappingSetting with ID ", id, "invalid."));
            }

            var mappingInfoId = id.Split('*')[0];
            var mappingInfoData = await DataReader.ReadRecordAsync<IntgMappingInfo>("INTG.MAPPING.INFO", mappingInfoId);
            if (mappingInfoData == null)
            {
                exception.AddError(new RepositoryError("Bad.Data", "Could not read INTG.MAPPING.INFO record " + mappingInfoId + "."));
                throw exception;
            }
            var mappingSetting = await BuildMappingSettingOptionsAsync(mappingSettingsData, mappingInfoData, bypassCache);

            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }

            return mappingSetting;
        }

        #region Build MappingSettingsOptions

        /// <summary>
        /// Build from a Collection of Data Contracts
        /// </summary>
        /// <param name="sources">INTG.MAPPING.SETTINGS data contracts</param>
        /// <param name="intgMappingInfos">INTG.MAPPING.INFO data contracts</param>
        /// <param name="bypassCache">bypass chache flag</param>
        /// <returns></returns>
        private async Task<IEnumerable<MappingSettingsOptions>> BuildMappingSettingsOptionsAsync(Collection<IntgMappingSettings> sources, Collection<IntgMappingInfo> intgMappingInfos, bool bypassCache = false)
        {
            var mappingSettingsOptionsCollection = new List<MappingSettingsOptions>();
            foreach (var source in sources)
            {
                // Extract the one INTG.MAPPING.INFO data contract for the one INTG.MAPPING.SETTING data contract
                var matchingIntgMappingInfo = intgMappingInfos.Where(im => (im.Recordkey == source.Recordkey.Split('*')[0]));
                foreach (var intgMappingInfo in matchingIntgMappingInfo)
                {
                    mappingSettingsOptionsCollection.Add(await BuildMappingSettingOptionsAsync(source, intgMappingInfo, bypassCache));
                }
            }
            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }
            return mappingSettingsOptionsCollection.AsEnumerable();
        }

        /// <summary>
        /// Build a single mapping-settings-options entry
        /// </summary>
        /// <param name="source">INTG.MAPPING.SETTINGS data contract</param>
        /// <param name="intgMappingInfo">INTG.MAPPING.INFO data contract</param>
        /// <param name="bypassCache">bypass cache flag</param>
        /// <returns></returns>
        private async Task<MappingSettingsOptions> BuildMappingSettingOptionsAsync(IntgMappingSettings source, IntgMappingInfo intgMappingInfo, bool bypassCache = false)
        {
            if (source == null)
            {
                exception.AddError(new RepositoryError("Bad.Data", "Selected, but can not read INTG.MAPPING.SETTINGS record."));
                return null;
            }

            MappingSettingsOptions mappingSettingOptions = null;
            try
            {
                /// switch on IMN.ETHOS.ENUMS.VALCODE.ID
                /// 
                var enumerations = new List<string>();
                if (intgMappingInfo.ImnEthosEnumsValcodeId != null)
                {                    
                    switch (intgMappingInfo.ImnEthosEnumsValcodeId.ToUpperInvariant())
                    {
                        case "INTG.EMAIL.TYPES":
                            enumerations = await GetAllValcodeItemDescriptionsAsync("CORE.VALCODES", 
                                intgMappingInfo.ImnEthosEnumsValcodeId.ToUpperInvariant(), bypassCache);
                            break;
                        case "INTG.PHONE.TYPES":
                            enumerations = await GetAllValcodeItemDescriptionsAsync("CORE.VALCODES",
                                intgMappingInfo.ImnEthosEnumsValcodeId.ToUpperInvariant(), bypassCache);
                            break;
                        case "INTG.RESTR.CATEGORIES":
                            enumerations = await GetAllValcodeItemDescriptionsAsync("CORE.VALCODES",
                                intgMappingInfo.ImnEthosEnumsValcodeId.ToUpperInvariant(), bypassCache);
                            break;
                        case "INTG.ADREL.TYPES":
                            enumerations = await GetAllValcodeItemDescriptionsAsync("CORE.VALCODES",
                                intgMappingInfo.ImnEthosEnumsValcodeId.ToUpperInvariant(), bypassCache);
                            break;
                        case "INTG.PERSON.RELATION.TYPES":
                            enumerations = await GetAllValcodeItemDescriptionsAsync("CORE.VALCODES",
                                intgMappingInfo.ImnEthosEnumsValcodeId.ToUpperInvariant(), bypassCache);
                            break;
                        case "INTG.MIL.STATUSES":
                            enumerations = await GetAllValcodeItemDescriptionsAsync("CORE.VALCODES",
                                intgMappingInfo.ImnEthosEnumsValcodeId.ToUpperInvariant(), bypassCache);
                            break;
                        case "INTG.SOCIAL.MEDIA.NETWORKS":
                            enumerations = await GetAllValcodeItemDescriptionsAsync("CORE.VALCODES",
                                intgMappingInfo.ImnEthosEnumsValcodeId.ToUpperInvariant(), bypassCache);
                            break;
                        case "INTG.VISA.TYPES":
                            enumerations = await GetAllValcodeItemDescriptionsAsync("CORE.VALCODES",
                                intgMappingInfo.ImnEthosEnumsValcodeId.ToUpperInvariant(), bypassCache);
                            break;
                        case "INTG.REHIRE.ELIGIBILITY":
                            enumerations = await GetAllValcodeItemDescriptionsAsync("HR.VALCODES",
                                intgMappingInfo.ImnEthosEnumsValcodeId.ToUpperInvariant(), bypassCache);
                            break;
                        case "INTG.HR.STATUSES":
                            enumerations = await GetAllValcodeItemDescriptionsAsync("HR.VALCODES",
                                intgMappingInfo.ImnEthosEnumsValcodeId.ToUpperInvariant(), bypassCache);
                            break;
                        case "INTG.SECTION.STATUSES":
                            enumerations = await GetAllValcodeItemDescriptionsAsync("ST.VALCODES",
                                intgMappingInfo.ImnEthosEnumsValcodeId.ToUpperInvariant(), bypassCache);
                            break;
                        case "INTG.SESS.CATEGORIES":
                            enumerations = await GetAllValcodeItemDescriptionsAsync("ST.VALCODES",
                                intgMappingInfo.ImnEthosEnumsValcodeId.ToUpperInvariant(), bypassCache);
                            break;
                        case "INTG.ROOM.TYPES":
                            enumerations = await GetAllValcodeItemDescriptionsAsync("CORE.VALCODES",
                                intgMappingInfo.ImnEthosEnumsValcodeId.ToUpperInvariant(), bypassCache);
                            break;
                        case "INTG.CONTACT.MEASURES":
                            enumerations = await GetAllValcodeItemDescriptionsAsync("ST.VALCODES",
                                intgMappingInfo.ImnEthosEnumsValcodeId.ToUpperInvariant(), bypassCache);
                            break;
                        case "INTG.AWARD.CATEGORY.NAMES":
                            enumerations = await GetAllValcodeItemDescriptionsAsync("ST.VALCODES",
                                intgMappingInfo.ImnEthosEnumsValcodeId.ToUpperInvariant(), bypassCache);
                            break;
                        case "INTG.ROOM.ASSIGN.STATUSES":
                            enumerations = await GetAllValcodeItemDescriptionsAsync("ST.VALCODES",
                                intgMappingInfo.ImnEthosEnumsValcodeId.ToUpperInvariant(), bypassCache);
                            break;
                        case "INTG.ROOM.REQUEST.STATUSES":
                            enumerations = await GetAllValcodeItemDescriptionsAsync("ST.VALCODES",
                                intgMappingInfo.ImnEthosEnumsValcodeId.ToUpperInvariant(), bypassCache);
                            break;
                        case "INTG.MEAL.PLAN.STATUSES":
                            enumerations = await GetAllValcodeItemDescriptionsAsync("ST.VALCODES",
                                intgMappingInfo.ImnEthosEnumsValcodeId.ToUpperInvariant(), bypassCache);
                            break;
                    }
                }


                mappingSettingOptions = new MappingSettingsOptions(source.RecordGuid, source.Recordkey, intgMappingInfo.ImnCollMappingTitle)
                {
                    // Add entity fields from each data contract
                    EthosResource = intgMappingInfo.ImnEthosResource,
                    EthosPropertyName = intgMappingInfo.ImnEthosPropertyName,
                    Enumerations = enumerations
                };
            }
            catch (Exception ex)
            {
                exception.AddError(new RepositoryError("Missing.Required.Property", "Unable to build mapping setting option.  " + ex.Message)
                {
                    Id = source.RecordGuid,
                    SourceId = source.Recordkey
                });
            }
            return mappingSettingOptions;
        }
        #endregion Build MappingSettingsOptions
        #endregion GET mapping-settings-options

        #region Update Mapping Settings

        public async Task<MappingSettings> UpdateMappingSettingsAsync(MappingSettings mappingSettings)
        {
            if (mappingSettings == null)
            {
                throw new ArgumentNullException("mappingSettingsEntity", "Must provide a mappingSettingsEntity to update.");
            }

            if (string.IsNullOrEmpty(mappingSettings.Guid))
            {
                throw new ArgumentNullException("mappingSettingsEntity", "Must provide the guid of the mappingSettingsEntity to update.");
            }

            // verify the GUID exists to perform an update.  If not, perform a create instead
            var mappingSettingsId = await GetMappingSettingsIdFromGuidAsync(mappingSettings.Guid);

            if (mappingSettingsId != null && !string.IsNullOrEmpty(mappingSettingsId))
            {
                var updateRequest = BuildUpdateRequest(mappingSettings);

                // write the  data
                var updateResponse = await transactionInvoker.ExecuteAsync<UpdateMappingSettingsRequest, UpdateMappingSettingsResponse>(updateRequest);

                if (updateResponse.MappingSettingErrors.Any())
                {
                    var errorMessage = string.Format("Error(s) occurred updating mapping-settings '{0}'", mappingSettings.Guid);
                    var exception = new RepositoryException(errorMessage);
                    updateResponse.MappingSettingErrors.ForEach(e => exception.AddError(new RepositoryError("Validation.Exception", string.Concat(e.ErrorCodes, ": ", e.ErrorMessages))
                    {
                        Id = mappingSettings.Guid,
                        SourceId = mappingSettingsId
                    }
                    ));
                    logger.Error(errorMessage);
                    throw exception;
                }
            }
            // get the updated entity from the database
            return await GetMappingSettingsByGuidAsync(mappingSettings.Guid, true);
        }

        /// <summary>
        /// Create an ApplicationSupportingItemRequest from a domain entity
        /// </summary>
        /// <param name="mappingSettingssEntity">supporting item domain entity</param>
        /// <returns>UpdateApplicationSupportingItemRequest transaction object</returns>
        private UpdateMappingSettingsRequest BuildUpdateRequest(MappingSettings mappingSettingsEntity)
        {
            var request = new UpdateMappingSettingsRequest()
            {
                Guid = mappingSettingsEntity.Guid,
                MappingId = mappingSettingsEntity.Code,
                Title = mappingSettingsEntity.Description,
                Resource = mappingSettingsEntity.EthosResource,
                Description = mappingSettingsEntity.EthosPropertyName,
                Enumeration = mappingSettingsEntity.Enumeration,
                SourceTitle = mappingSettingsEntity.SourceTitle,
                SourceValue = mappingSettingsEntity.SourceValue
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

        #region Helper Methods

        private async Task<List<string>> GetRoomTypeAsync(string sourceValue, bool bypassCache)
        {
            if (string.IsNullOrEmpty(sourceValue))
            {
                return new List<string>();
            }

            if (bypassCache)
            {
                var roomType = await DataReader.ReadRecordAsync<DataContracts.RoomTypes>(sourceValue);
                if (roomType == null)
                {
                    throw new KeyNotFoundException(string.Format("Could not read : '{0}' from ROOM.TYPES.", sourceValue));
                }
                else
                {
                    List<string> roomTypeInfo = new List<string>() { roomType.RmtpDescription, roomType.RmtpIntgRoomType };
                    return roomTypeInfo;
                }
            }
            var dictRoomTypes = await GetAllRoomTypesAsync(bypassCache);

            if (!dictRoomTypes.ContainsKey(sourceValue))
            {
                throw new KeyNotFoundException(string.Format("Could not find valid : '{0}' in ROOM.TYPES.", sourceValue));
            }
            else
            {
                var sourceInfo = new List<string>();
                dictRoomTypes.TryGetValue(sourceValue, out sourceInfo);
                if (!sourceInfo.Any())
                {
                    throw new KeyNotFoundException(string.Format("Could not read : '{0}' from ROOM.TYPES.", sourceValue));
                }
                else
                {
                    return sourceInfo;
                }
            }
        }

        /// <summary>
        /// Retrieve all ROOM.TYPES records from cache and return dictionary
        /// of key and list containing description and mapped integration code.
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns>"Dictionary of key value pair: key and list with description and mapped integration code.</returns>
        private async Task<Dictionary<string, List<string>>> GetAllRoomTypesAsync(bool bypassCache)
        {
            Dictionary<string, List<string>> dictRoomTypes = new Dictionary<string, List<string>>();

            var cacheControlKey = "AllRoomTypesMappingSettings";
            if (bypassCache && ContainsKey(BuildFullCacheKey(cacheControlKey)))
            {
                ClearCache(new List<string> { cacheControlKey });
            }
            var allRoomTypes = await GetOrAddToCacheAsync<IEnumerable<DataContracts.RoomTypes>>(cacheControlKey,
                async () =>
                {
                    var roomTypes = await DataReader.BulkReadRecordAsync<DataContracts.RoomTypes>("ROOM.TYPES", "");
                    if (roomTypes == null)
                    {
                        logger.Info("Unable to access ROOM.TYPES from database.");
                        roomTypes = new Collection<DataContracts.RoomTypes>();
                    }
                    return roomTypes;
                }, Level1CacheTimeoutValue);

            if (allRoomTypes != null && allRoomTypes.Any())
            {
                foreach (var roomType in allRoomTypes)
                {
                    if (roomType != null && !string.IsNullOrEmpty(roomType.RmtpDescription))
                    {
                        var dictKey = roomType.Recordkey;
                        var dictValue = new List<string>() { roomType.RmtpDescription, roomType.RmtpIntgRoomType };
                        if (!string.IsNullOrEmpty(dictKey) && !dictRoomTypes.ContainsKey(dictKey))
                        {
                            dictRoomTypes.Add(dictKey, dictValue);
                        }
                    }
                }
            }

            return dictRoomTypes;
        }

        private async Task<List<string>> GetRelationTypeAsync(string sourceValue, bool bypassCache)
        {
            if (string.IsNullOrEmpty(sourceValue))
            {
                return new List<string>();
            }

            if (bypassCache)
            {
                var relationType = await DataReader.ReadRecordAsync<RelationTypes>(sourceValue);
                if (relationType == null)
                {
                    throw new KeyNotFoundException(string.Format("Could not read : '{0}' from RELATION.TYPES.", sourceValue));
                }
                else
                {
                    var relationTypeInfo = new List<string>() { relationType.ReltyDesc, relationType.ReltyIntgPersonRelType, relationType.ReltyIntgMaleRelType, relationType.ReltyIntgFemaleRelType };
                    return relationTypeInfo;
                }
            }
            var dictRelationTypes = await GetAllRelationTypesAsync(bypassCache);

            if (!dictRelationTypes.ContainsKey(sourceValue))
            {
                throw new KeyNotFoundException(string.Format("Could not find valid : '{0}' in RELATION.TYPES.", sourceValue));
            }
            else
            {
                var sourceInfo = new List<string>();
                dictRelationTypes.TryGetValue(sourceValue, out sourceInfo);
                if (!sourceInfo.Any())
                {
                    throw new KeyNotFoundException(string.Format("Could not read : '{0}' from RELATION.TYPES.", sourceValue));
                }
                else
                {
                    return sourceInfo;
                }
            }
        }

        /// <summary>
        /// Retrieve all RELATION.TYPES records from cache and return dictionary
        /// of key and list containing description and mapped integration code.
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns>"Dictionary of key value pair: key and list with description and mapped integration code.</returns>
        private async Task<Dictionary<string, List<string>>> GetAllRelationTypesAsync(bool bypassCache)
        {
            Dictionary<string, List<string>> dictRelationTypes = new Dictionary<string, List<string>>();

            var cacheControlKey = "AllRelationTypesMappingSettings";
            if (bypassCache && ContainsKey(BuildFullCacheKey(cacheControlKey)))
            {
                ClearCache(new List<string> { cacheControlKey });
            }
            var allRelationTypes = await GetOrAddToCacheAsync<IEnumerable<RelationTypes>>(cacheControlKey,
                async () =>
                {
                    var relationTypes = await DataReader.BulkReadRecordAsync<RelationTypes>("RELATION.TYPES", "");
                    if (relationTypes == null)
                    {
                        logger.Info("Unable to access RELATION.TYPES from database.");
                        relationTypes = new Collection<RelationTypes>();
                    }
                    return relationTypes;
                }, Level1CacheTimeoutValue);

            if (allRelationTypes != null && allRelationTypes.Any())
            {
                foreach (var relationType in allRelationTypes)
                {
                    if (relationType != null && !string.IsNullOrEmpty(relationType.ReltyDesc))
                    {
                        var dictKey = relationType.Recordkey;
                        var dictValue = new List<string>() { relationType.ReltyDesc, relationType.ReltyIntgPersonRelType, relationType.ReltyIntgMaleRelType, relationType.ReltyIntgFemaleRelType };
                        if (!string.IsNullOrEmpty(dictKey) && !dictRelationTypes.ContainsKey(dictKey))
                        {
                            dictRelationTypes.Add(dictKey, dictValue);
                        }
                    }
                }
            }

            return dictRelationTypes;
        }

        private async Task<List<string>> GetRestrictionAsync(string sourceValue, bool bypassCache)
        {
            if (string.IsNullOrEmpty(sourceValue))
            {
                return new List<string>();
            }

            if (bypassCache)
            {
                var restriction = await DataReader.ReadRecordAsync<Restrictions>(sourceValue);
                if (restriction == null)
                {
                    throw new KeyNotFoundException(string.Format("Could not read : '{0}' from RESTRICTIONS.", sourceValue));
                }
                else
                {
                    var restrictionInfo = new List<string>() { restriction.RestDesc, restriction.RestIntgCategory };
                    return restrictionInfo;
                }
            }
            var dictRestrictions = await GetAllRestrictionsAsync(bypassCache);

            if (!dictRestrictions.ContainsKey(sourceValue))
            {
                throw new KeyNotFoundException(string.Format("Could not find valid : '{0}' in RESTRICTIONS.", sourceValue));
            }
            else
            {
                var sourceInfo = new List<string>();
                dictRestrictions.TryGetValue(sourceValue, out sourceInfo);
                if (!sourceInfo.Any())
                {
                    throw new KeyNotFoundException(string.Format("Could not read :'{0}' from RESTRICTIONS.", sourceValue));
                }
                else
                {
                    return sourceInfo;
                }
            }
        }

        /// <summary>
        /// Retrieve all RESTRICTIONS records from cache and return dictionary
        /// of key and list containing description and mapped integration code.
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns>"Dictionary of key value pair: key and list with description and mapped integration code.</returns>
        private async Task<Dictionary<string, List<string>>> GetAllRestrictionsAsync(bool bypassCache)
        {
            Dictionary<string, List<string>> dictRestrictions = new Dictionary<string, List<string>>();

            var cacheControlKey = "AllRestrictionsMappingSettings";
            if (bypassCache && ContainsKey(BuildFullCacheKey(cacheControlKey)))
            {
                ClearCache(new List<string> { cacheControlKey });
            }
            var allRestrictions = await GetOrAddToCacheAsync<IEnumerable<Restrictions>>(cacheControlKey,
                async () =>
                {
                    var restrictions = await DataReader.BulkReadRecordAsync<Restrictions>("RESTRICTIONS", "");
                    if (restrictions == null)
                    {
                        logger.Info("Unable to access RESTRICTIONS from database.");
                        restrictions = new Collection<Restrictions>();
                    }
                    return restrictions;
                }, Level1CacheTimeoutValue);

            if (allRestrictions != null && allRestrictions.Any())
            {
                foreach (var restriction in allRestrictions)
                {
                    if (restriction != null && !string.IsNullOrEmpty(restriction.RestDesc))
                    {
                        var dictKey = restriction.Recordkey;
                        var dictValue = new List<string>() { restriction.RestDesc, restriction.RestIntgCategory };
                        if (!string.IsNullOrEmpty(dictKey) && !dictRestrictions.ContainsKey(dictKey))
                        {
                            dictRestrictions.Add(dictKey, dictValue);
                        }
                    }
                }
            }

            return dictRestrictions;
        }

        private async Task<List<string>> GetSessionAsync(string sourceValue, bool bypassCache)
        {
            if (string.IsNullOrEmpty(sourceValue))
            {
                return new List<string>();
            }

            if (bypassCache)
            {
                var session = await DataReader.ReadRecordAsync<SessionsBase>(sourceValue);
                if (session == null)
                {
                    throw new KeyNotFoundException(string.Format("Could not read : '{0}' from SESSIONS.", sourceValue));
                }
                else
                {
                    var sessionInfo = new List<string>() { session.SessDesc, session.SessIntgCategory };
                    return sessionInfo;
                }
            }
            var dictSessions = await GetAllSessionsAsync(bypassCache);

            if (!dictSessions.ContainsKey(sourceValue))
            {
                throw new KeyNotFoundException(string.Format("Could not find valid : '{0}' in SESSIONS.", sourceValue));
            }
            else
            {
                var sourceInfo = new List<string>();
                dictSessions.TryGetValue(sourceValue, out sourceInfo);
                if (!sourceInfo.Any())
                {
                    throw new KeyNotFoundException(string.Format("Could not read : '{0}' from SESSIONS.", sourceValue));
                }
                else
                {
                    return sourceInfo;
                }
            }
        }

        /// <summary>
        /// Retrieve all SESSIONS records from cache and return dictionary
        /// of key and list containing description and mapped integration code.
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns>"Dictionary of key value pair: key and list with description and mapped integration code.</returns>
        private async Task<Dictionary<string, List<string>>> GetAllSessionsAsync(bool bypassCache)
        {
            Dictionary<string, List<string>> dictSessions = new Dictionary<string, List<string>>();

            var cacheControlKey = "AllSessionsMappingSettings";
            if (bypassCache && ContainsKey(BuildFullCacheKey(cacheControlKey)))
            {
                ClearCache(new List<string> { cacheControlKey });
            }
            var allSessions = await GetOrAddToCacheAsync<IEnumerable<SessionsBase>>(cacheControlKey,
                async () =>
                {
                    var sessions = await DataReader.BulkReadRecordAsync<SessionsBase>("SESSIONS", "");
                    if (sessions == null)
                    {
                        logger.Info("Unable to access SESSIONS from database.");
                        sessions = new Collection<SessionsBase>();
                    }
                    return sessions;
                }, Level1CacheTimeoutValue);

            if (allSessions != null && allSessions.Any())
            {
                foreach (var session in allSessions)
                {
                    if (session != null && !string.IsNullOrEmpty(session.SessDesc))
                    {
                        var dictKey = session.Recordkey;
                        var dictValue = new List<string>() { session.SessDesc, session.SessIntgCategory };
                        if (!string.IsNullOrEmpty(dictKey) && !dictSessions.ContainsKey(dictKey))
                        {
                            dictSessions.Add(dictKey, dictValue);
                        }
                    }
                }
            }

            return dictSessions;
        }

        private async Task<List<string>> GetAwardCategoryAsync(string sourceValue, bool bypassCache)
        {
            if (string.IsNullOrEmpty(sourceValue))
            {
                return new List<string>();
            }

            if (bypassCache)
            {
                var awardCategory = await DataReader.ReadRecordAsync<AwardCategoriesBase>(sourceValue);
                if (awardCategory == null)
                {
                    throw new KeyNotFoundException(string.Format("Could not read : '{0}' from AWARD.CATEGORIES.", sourceValue));
                }
                else
                {
                    var awardCategoryInfo = new List<string>() { awardCategory.AcDescription, awardCategory.AcIntgName };
                    return awardCategoryInfo;
                }
            }
            var dictAwardCategories = await GetAllAwardCategoriesAsync(bypassCache);

            if (!dictAwardCategories.ContainsKey(sourceValue))
            {
                throw new KeyNotFoundException(string.Format("Could not find valid : '{0}' in AWARD.CATEGORIES.", sourceValue));
            }
            else
            {
                var sourceInfo = new List<string>();
                dictAwardCategories.TryGetValue(sourceValue, out sourceInfo);
                if (!sourceInfo.Any())
                {
                    throw new KeyNotFoundException(string.Format("Could not read : '{0}' from AWARD.CATEGORIES.", sourceValue));
                }
                else
                {
                    return sourceInfo;
                }
            }
        }

        /// <summary>
        /// Retrieve all AWARD.CATEGORIES records from cache and return dictionary
        /// of key and list containing description and mapped integration code.
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns>"Dictionary of key value pair: key and list with description and mapped integration code.</returns>
        private async Task<Dictionary<string, List<string>>> GetAllAwardCategoriesAsync(bool bypassCache)
        {
            Dictionary<string, List<string>> dictAwardCategories = new Dictionary<string, List<string>>();

            var cacheControlKey = "AllAwardCategoriesMappingSettings";
            if (bypassCache && ContainsKey(BuildFullCacheKey(cacheControlKey)))
            {
                ClearCache(new List<string> { cacheControlKey });
            }
            var allAwardCategories = await GetOrAddToCacheAsync<IEnumerable<AwardCategoriesBase>>(cacheControlKey,
                async () =>
                {
                    var sessions = await DataReader.BulkReadRecordAsync<AwardCategoriesBase>("AWARD.CATEGORIES", "");
                    if (sessions == null)
                    {
                        logger.Info("Unable to access AWARD.CATEGORIES from database.");
                        sessions = new Collection<AwardCategoriesBase>();
                    }
                    return sessions;
                }, Level1CacheTimeoutValue);

            if (allAwardCategories != null && allAwardCategories.Any())
            {
                foreach (var awardCategory in allAwardCategories)
                {
                    if (awardCategory != null && !string.IsNullOrEmpty(awardCategory.AcDescription))
                    {
                        var dictKey = awardCategory.Recordkey;
                        var dictValue = new List<string>() { awardCategory.AcDescription, awardCategory.AcIntgName };
                        if (!string.IsNullOrEmpty(dictKey) && !dictAwardCategories.ContainsKey(dictKey))
                        {
                            dictAwardCategories.Add(dictKey, dictValue);
                        }
                    }
                }
            }

            return dictAwardCategories;
        }

        private async Task<List<string>> GetEmailTypesAsync(string sourceValue, bool bypassCache)
        {
            if (string.IsNullOrEmpty(sourceValue))
            {
                return new List<string>();
            }

            var dictValcodes = await GetAllValcodeItemsAsync("CORE.VALCODES", "PERSON.EMAIL.TYPES", bypassCache);

            if (!dictValcodes.ContainsKey(sourceValue))
            {
                throw new KeyNotFoundException(string.Format("Could not find '{0}' in PERSON.EMAIL.TYPES.", sourceValue));
            }
            else
            {
                List<string> source = null;
                dictValcodes.TryGetValue(sourceValue, out source);
                var sourceTitle = source[0];
                var specialProcessing3 = source[1];
                if (string.IsNullOrEmpty(sourceTitle))
                {
                    throw new KeyNotFoundException(string.Format("Could not read '{0}' from PERSON.EMAIL.TYPES.", sourceValue));
                }
                else
                {
                    return source;
                }
            }
        }

        private async Task<List<string>> GetPhoneTypesAsync(string sourceValue, bool bypassCache)
        {
            if (string.IsNullOrEmpty(sourceValue))
            {
                return new List<string>();
            }

            var dictValcodes = await GetAllValcodeItemsAsync("CORE.VALCODES", "PHONE.TYPES", bypassCache);

            if (!dictValcodes.ContainsKey(sourceValue))
            {
                throw new KeyNotFoundException(string.Format("Could not find '{0}' in PHONE.TYPES.", sourceValue));
            }
            else
            {
                List<string> source = null;
                dictValcodes.TryGetValue(sourceValue, out source);
                var sourceTitle = source[0];
                var specialProcessing3 = source[1];
                if (string.IsNullOrEmpty(sourceTitle))
                {
                    throw new KeyNotFoundException(string.Format("Could not read '{0}' from PHONE.TYPES.", sourceValue));
                }
                else
                {
                    return source;
                }
            }
        }

        private async Task<List<string>> GetAdrelTypesAsync(string sourceValue, bool bypassCache)
        {
            if (string.IsNullOrEmpty(sourceValue))
            {
                return new List<string>();
            }

            var dictValcodes = await GetAllValcodeItemsAsync("CORE.VALCODES", "ADREL.TYPES", bypassCache);

            if (!dictValcodes.ContainsKey(sourceValue))
            {
                throw new KeyNotFoundException(string.Format("Could not find : '{0}' in ADREL.TYPES.", sourceValue));
            }
            else
            {
                List<string> source = null;
                dictValcodes.TryGetValue(sourceValue, out source);
                var sourceTitle = source[0];
                var specialProcessing3 = source[1];
                if (string.IsNullOrEmpty(sourceTitle))
                {
                    throw new KeyNotFoundException(string.Format("Could not read : '{0}' from ADREL.TYPES.", sourceValue));
                }
                else
                {
                    return source;
                }
            }
        }

        private async Task<List<string>> GetMilStatusesAsync(string sourceValue, bool bypassCache)
        {
            if (string.IsNullOrEmpty(sourceValue))
            {
                return new List<string>();
            }

            var dictValcodes = await GetAllValcodeItemsAsync("CORE.VALCODES", "MIL.STATUSES", bypassCache);

            if (!dictValcodes.ContainsKey(sourceValue))
            {
                throw new KeyNotFoundException(string.Format("Could not find : '{0}' in MIL.STATUSES.", sourceValue));
            }
            else
            {
                List<string> source = null;
                dictValcodes.TryGetValue(sourceValue, out source);
                var sourceTitle = source[0];
                var specialProcessing3 = source[1];
                if (string.IsNullOrEmpty(sourceTitle))
                {
                    throw new KeyNotFoundException(string.Format("Could not read : '{0}' from MIL.STATUSES.", sourceValue));
                }
                else
                {
                    return source;
                }
            }
        }

        private async Task<List<string>> GetSocialMediaNetworks(string sourceValue, bool bypassCache)
        {
            if (string.IsNullOrEmpty(sourceValue))
            {
                return new List<string>();
            }

            var dictValcodes = await GetAllValcodeItemsAsync("CORE.VALCODES", "SOCIAL.MEDIA.NETWORKS", bypassCache);

            if (!dictValcodes.ContainsKey(sourceValue))
            {
                throw new KeyNotFoundException(string.Format("Could not find : '{0}' in SOCIAL.MEDIA.NETWORKS.", sourceValue));
            }
            else
            {
                List<string> source = null;
                dictValcodes.TryGetValue(sourceValue, out source);
                var sourceTitle = source[0];
                var specialProcessing3 = source[1];
                if (string.IsNullOrEmpty(sourceTitle))
                {
                    throw new KeyNotFoundException(string.Format("Could not read : '{0}' from SOCIAL.MEDIA.NETWORKS.", sourceValue));
                }
                else
                {
                    return source;
                }
            }
        }

        private async Task<List<string>> GetVisaTypesAsync(string sourceValue, bool bypassCache)
        {
            if (string.IsNullOrEmpty(sourceValue))
            {
                return new List<string>();
            }

            var dictValcodes = await GetAllValcodeItemsAsync("CORE.VALCODES", "VISA.TYPES", bypassCache);

            if (!dictValcodes.ContainsKey(sourceValue))
            {
                throw new KeyNotFoundException(string.Format("Could not find : '{0}' in VISA.TYPES.", sourceValue));
            }
            else
            {
                List<string> source = null;
                dictValcodes.TryGetValue(sourceValue, out source);
                var sourceTitle = source[0];
                var specialProcessing3 = source[1];
                if (string.IsNullOrEmpty(sourceTitle))
                {
                    throw new KeyNotFoundException(string.Format("Could not read : '{0}' from VISA.TYPES.", sourceValue));
                }
                else
                {
                    return source;
                }
            }
        }

        private async Task<List<string>> GetRoomTypesAsync(string sourceValue, bool bypassCache)
        {
            if (string.IsNullOrEmpty(sourceValue))
            {
                return new List<string>();
            }

            var dictValcodes = await GetAllValcodeItemsAsync("CORE.VALCODES", "ROOM.TYPES", bypassCache);

            if (!dictValcodes.ContainsKey(sourceValue))
            {
                throw new KeyNotFoundException(string.Format("Could not find : '{0}' in ROOM.TYPES.", sourceValue));
            }
            else
            {
                List<string> source = null;
                dictValcodes.TryGetValue(sourceValue, out source);
                var sourceTitle = source[0];
                var specialProcessing3 = source[1];
                if (string.IsNullOrEmpty(sourceTitle))
                {
                    throw new KeyNotFoundException(string.Format("Could not read : '{0}' from ROOM.TYPES.", sourceValue));
                }
                else
                {
                    return source;
                }
            }
        }

        private async Task<List<string>> GetSectionStatusesAsync(string sourceValue, bool bypassCache)
        {
            if (string.IsNullOrEmpty(sourceValue))
            {
                return new List<string>();
            }

            var dictValcodes = await GetAllValcodeItemsAsync("ST.VALCODES", "SECTION.STATUSES", bypassCache);

            if (!dictValcodes.ContainsKey(sourceValue))
            {
                throw new KeyNotFoundException(string.Format("Could not find : '{0}' in SECTION.STATUSES.", sourceValue));
            }
            else
            {
                List<string> source = null;
                dictValcodes.TryGetValue(sourceValue, out source);
                var sourceTitle = source[0];
                var specialProcessing3 = source[1];
                if (string.IsNullOrEmpty(sourceTitle))
                {
                    throw new KeyNotFoundException(string.Format("Could not read : '{0}' from SECTION.STATUSES.", sourceValue));
                }
                else
                {
                    return source;
                }
            }
        }

        private async Task<List<string>> GetContactMeasuresAsync(string sourceValue, bool bypassCache)
        {
            if (string.IsNullOrEmpty(sourceValue))
            {
                return new List<string>();
            }

            var dictValcodes = await GetAllValcodeItemsAsync("ST.VALCODES", "CONTACT.MEASURES", bypassCache);

            if (!dictValcodes.ContainsKey(sourceValue))
            {
                throw new KeyNotFoundException(string.Format("Could not find : '{0}' in CONTACT.MEASURES.", sourceValue));
            }
            else
            {
                List<string> source = null;
                dictValcodes.TryGetValue(sourceValue, out source);
                var sourceTitle = source[0];
                var specialProcessing3 = source[1];
                if (string.IsNullOrEmpty(sourceTitle))
                {
                    throw new KeyNotFoundException(string.Format("Could not read : '{0}' from CONTACT.MEASURES.", sourceValue));
                }
                else
                {
                    return source;
                }
            }
        }

        private async Task<List<string>> GetRoomAssignStatusesAsync(string sourceValue, bool bypassCache)
        {
            if (string.IsNullOrEmpty(sourceValue))
            {
                return new List<string>();
            }

            var dictValcodes = await GetAllValcodeItemsAsync("ST.VALCODES", "ROOM.ASSIGN.STATUSES", bypassCache);

            if (!dictValcodes.ContainsKey(sourceValue))
            {
                throw new KeyNotFoundException(string.Format("Could not find : '{0}' in ROOM.ASSIGN.STATUSES.", sourceValue));
            }
            else
            {
                List<string> source = null;
                dictValcodes.TryGetValue(sourceValue, out source);
                var sourceTitle = source[0];
                var specialProcessing3 = source[1];
                if (string.IsNullOrEmpty(sourceTitle))
                {
                    throw new KeyNotFoundException(string.Format("Could not read : '{0}' from ROOM.ASSIGN.STATUSES.", sourceValue));
                }
                else
                {
                    return source;
                }
            }
        }

        private async Task<List<string>> GetMealAssignStatusesAsync(string sourceValue, bool bypassCache)
        {
            if (string.IsNullOrEmpty(sourceValue))
            {
                return new List<string>();
            }

            var dictValcodes = await GetAllValcodeItemsAsync("ST.VALCODES", "MEAL.ASSIGN.STATUSES", bypassCache);

            if (!dictValcodes.ContainsKey(sourceValue))
            {
                throw new KeyNotFoundException(string.Format("Could not find : '{0}' in MEAL.ASSIGN.STATUSES.", sourceValue));
            }
            else
            {
                List<string> source = null;
                dictValcodes.TryGetValue(sourceValue, out source);
                var sourceTitle = source[0];
                var specialProcessing3 = source[1];
                if (string.IsNullOrEmpty(sourceTitle))
                {
                    throw new KeyNotFoundException(string.Format("Could not read : '{0}' from MEAL.ASSIGN.STATUSES.", sourceValue));
                }
                else
                {
                    return source;
                }
            }
        }

        private async Task<List<string>> GetRehireEligibilityCodesAsync(string sourceValue, bool bypassCache)
        {
            if (string.IsNullOrEmpty(sourceValue))
            {
                return new List<string>();
            }

            var dictValcodes = await GetAllValcodeItemsAsync("HR.VALCODES", "REHIRE.ELIGIBILITY.CODES", bypassCache);

            if (!dictValcodes.ContainsKey(sourceValue))
            {
                throw new KeyNotFoundException(string.Format("Could not find : '{0}' in REHIRE.ELIGIBILITY.CODES.", sourceValue));
            }
            else
            {
                List<string> source = null;
                dictValcodes.TryGetValue(sourceValue, out source);
                var sourceTitle = source[0];
                var specialProcessing3 = source[1];
                if (string.IsNullOrEmpty(sourceTitle))
                {
                    throw new KeyNotFoundException(string.Format("Could not read : '{0}' from REHIRE.ELIGIBILITY.CODES.", sourceValue));
                }
                else
                {
                    return source;
                }
            }
        }

        private async Task<List<string>> GetHrStatusesAsync(string sourceValue, bool bypassCache)
        {
            if (string.IsNullOrEmpty(sourceValue))
            {
                return new List<string>();
            }

            var dictValcodes = await GetAllValcodeItemsAsync("HR.VALCODES", "HR.STATUSES", bypassCache);

            if (!dictValcodes.ContainsKey(sourceValue))
            {
                throw new KeyNotFoundException(string.Format("Could not find : '{0}' in ROOM.ASSIGN.STATUSES.", sourceValue));
            }
            else
            {
                List<string> source = null;
                dictValcodes.TryGetValue(sourceValue, out source);
                var sourceTitle = source[0];
                if (source[1] != null)
                {
                    var specialProcessing3 = source[1];
                }
                if (string.IsNullOrEmpty(sourceTitle))
                {
                    throw new KeyNotFoundException(string.Format("Could not read : '{0}' from ROOM.ASSIGN.STATUSES.", sourceValue));
                }
                else
                {
                    return source;
                }
            }
        }

        private async Task<List<string>> GetIntgRestrCategoriesAsync(string sourceValue, bool bypassCache)
        {
            if (string.IsNullOrEmpty(sourceValue))
            {
                return new List<string>();
            }

            var dictValcodes = await GetAllValcodeItemsAsync("CORE.VALCODES", "INTG.RESTR.CATEGORIES", bypassCache);

            if (!dictValcodes.ContainsKey(sourceValue))
            {
                throw new KeyNotFoundException(string.Format("Could not find an enumeration for value: '{0}' from INTG.RESTR.CATEGORIES.", sourceValue));
            }
            else
            {
                List<string> source = null;
                dictValcodes.TryGetValue(sourceValue, out source);
                var sourceTitle = source[0];
                if (string.IsNullOrEmpty(sourceTitle))
                {
                    throw new KeyNotFoundException(string.Format("Could not find an enumeration for value: '{0}' from INTG.RESTR.CATEGORIES.", sourceValue));
                }
                else
                {
                    return source;
                }
            }
        }

        private async Task<List<string>> GetIntgPersonRelationTypesAsync(string sourceValue, bool bypassCache)
        {
            if (string.IsNullOrEmpty(sourceValue))
            {
                return new List<string>();
            }

            var dictValcodes = await GetAllValcodeItemsAsync("CORE.VALCODES", "INTG.PERSON.RELATION.TYPES", bypassCache);

            if (!dictValcodes.ContainsKey(sourceValue))
            {
                throw new KeyNotFoundException(string.Format("Could not find an enumeration for value: '{0}' from INTG.PERSON.RELATION.TYPES.", sourceValue));
            }
            else
            {
                List<string> source = null;
                dictValcodes.TryGetValue(sourceValue, out source);
                var sourceTitle = source[0];
                if (string.IsNullOrEmpty(sourceTitle))
                {
                    throw new KeyNotFoundException(string.Format("Could not find an enumeration for value: '{0}' from INTG.PERSON.RELATION.TYPES.", sourceValue));
                }
                else
                {
                    return source;
                }
            }
        }

        private async Task<List<string>> GetIntgRoomTypesAsync(string sourceValue, bool bypassCache)
        {
            if (string.IsNullOrEmpty(sourceValue))
            {
                return new List<string>();
            }

            var dictValcodes = await GetAllValcodeItemsAsync("CORE.VALCODES", "INTG.ROOM.TYPES", bypassCache);

            if (!dictValcodes.ContainsKey(sourceValue))
            {
                throw new KeyNotFoundException(string.Format("Could not find an enumeration for value: '{0}' from INTG.ROOM.TYPES.", sourceValue));
            }
            else
            {
                List<string> source = null;
                dictValcodes.TryGetValue(sourceValue, out source);
                var sourceTitle = source[0];
                if (string.IsNullOrEmpty(sourceTitle))
                {
                    throw new KeyNotFoundException(string.Format("Could not find an enumeration for value: '{0}' from INTG.ROOM.TYPES.", sourceValue));
                }
                else
                {
                    return source;
                }
            }
        }

        private async Task<List<string>> GetIntgSessCategoriesAsync(string sourceValue, bool bypassCache)
        {
            if (string.IsNullOrEmpty(sourceValue))
            {
                return new List<string>();
            }

            var dictValcodes = await GetAllValcodeItemsAsync("ST.VALCODES", "INTG.SESS.CATEGORIES", bypassCache);

            if (!dictValcodes.ContainsKey(sourceValue))
            {
                throw new KeyNotFoundException(string.Format("Could not find an enumeration for value: '{0}' from INTG.SESS.CATEGORIES.", sourceValue));
            }
            else
            {
                List<string> source = null;
                dictValcodes.TryGetValue(sourceValue, out source);
                var sourceTitle = source[0];
                if (string.IsNullOrEmpty(sourceTitle))
                {
                    throw new KeyNotFoundException(string.Format("Could not find an enumeration for value: '{0}' from INTG.SESS.CATEGORIES.", sourceValue));
                }
                else
                {
                    return source;
                }
            }
        }

        private async Task<List<string>> GetIntgAwardCategoryNamesAsync(string sourceValue, bool bypassCache)
        {
            if (string.IsNullOrEmpty(sourceValue))
            {
                return new List<string>();
            }

            var dictValcodes = await GetAllValcodeItemsAsync("ST.VALCODES", "INTG.AWARD.CATEGORY.NAMES", bypassCache);

            if (!dictValcodes.ContainsKey(sourceValue))
            {
                throw new KeyNotFoundException(string.Format("Could not find an enumeration for value: '{0}' from INTG.AWARD.CATEGORY.NAMES.", sourceValue));
            }
            else
            {
                List<string> source = null;
                dictValcodes.TryGetValue(sourceValue, out source);
                var sourceTitle = source[0];
                if (string.IsNullOrEmpty(sourceTitle))
                {
                    throw new KeyNotFoundException(string.Format("Could not find an enumeration for value: '{0}' from INTG.AWARD.CATEGORY.NAMES.", sourceValue));
                }
                else
                {
                    return source;
                }
            }
        }

        private async Task<Dictionary<string, List<string>>> GetAllValcodeItemsAsync(string entity, string valcodeTable, bool bypassCache)
        {
            Dictionary<string, List<string>> dictValcodeItems = new Dictionary<string, List<string>>();

            var cacheControlKey = string.Concat("AllValcodeTableMappingSettings", entity, valcodeTable);
            if (bypassCache && ContainsKey(BuildFullCacheKey(cacheControlKey)))
            {
                ClearCache(new List<string> { cacheControlKey });
            }
            return await GetOrAddToCacheAsync<Dictionary<string, List<string>>>(cacheControlKey,
                async () =>
                {
                    var valcode = await DataReader.ReadRecordAsync<Ellucian.Data.Colleague.DataContracts.ApplValcodes>(entity, valcodeTable);

                    if (valcode != null)
                    {
                        foreach (var row in valcode.ValsEntityAssociation)
                        {
                            var dictKey = row.ValInternalCodeAssocMember;
                            var dictValue = new List<string> { row.ValExternalRepresentationAssocMember, row.ValActionCode3AssocMember, row.ValActionCode4AssocMember };
                            if (!string.IsNullOrEmpty(dictKey) && dictValue.Any() && !dictValcodeItems.ContainsKey(dictKey))
                            {
                                dictValcodeItems.Add(dictKey, dictValue);
                            }
                        }
                    }
                    return dictValcodeItems;
                }, Level1CacheTimeoutValue);
        }

        private async Task<List<string>> GetAllValcodeItemDescriptionsAsync(string entity, string valcodeTable, bool bypassCache)
        {

            var dictValcodes = await GetAllValcodeItemsAsync(entity, valcodeTable, bypassCache);

            var descriptions = new List<string>();
            foreach (KeyValuePair<string, List<string>> entry in dictValcodes)
            {
                // do something with entry.Value or entry.Key
                descriptions.Add(entry.Value[0]);
            }
            return descriptions;
        }

        #endregion
    }
}