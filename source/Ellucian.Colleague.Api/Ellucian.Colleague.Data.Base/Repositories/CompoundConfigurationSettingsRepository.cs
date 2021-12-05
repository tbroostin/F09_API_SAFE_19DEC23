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
    public class CompoundConfigurationSettingsRepository : BaseColleagueRepository, ICompoundConfigurationSettingsRepository
    {
        public RepositoryException exception = new RepositoryException();
        public static char _VM = Convert.ToChar(DynamicArray.VM);
        public static char _SM = Convert.ToChar(DynamicArray.SM);
        private readonly int _readSize;

        public CompoundConfigurationSettingsRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            CacheTimeout = Level1CacheTimeoutValue;
            this._readSize = ((apiSettings != null) && (apiSettings.BulkReadSize > 0)) ? apiSettings.BulkReadSize : 5000;
        }

        /// <summary>
        /// Get a collection of IntgCmpdConfigSettings
        /// </summary>
        /// <param name="bypassCache">Bypass cache flag</param>
        /// <returns>Collection of IntgCmpdConfigSettings</returns>
        public async Task<IEnumerable<CompoundConfigurationSettings>> GetCompoundConfigurationSettingsAsync(bool bypassCache)
        {
            if (bypassCache && ContainsKey(BuildFullCacheKey("AllIntgCmpdConfigSettings")))
            {
                ClearCache(new List<string> { "AllIntgCmpdConfigSettings" });
            }
            return await GetOrAddToCacheAsync<IEnumerable<CompoundConfigurationSettings>>("AllIntgCmpdConfigSettings",
                async () =>
                {
                    var compoundSettingsIds = await DataReader.SelectAsync("INTG.CMPD.CONFIG.SETTINGS", "");
                    var compoundSettingsDataContracts = await DataReader.BulkReadRecordAsync<IntgCmpdConfigSettings>("INTG.CMPD.CONFIG.SETTINGS", compoundSettingsIds);
                    return await BuildCompoundConfigurationSettingsAsync(compoundSettingsDataContracts, bypassCache);

                }, Level1CacheTimeoutValue);
        }


        /// <summary>
        /// Get a single Configuration Settings domain entity from an compound configuration settings guid.
        /// </summary>
        /// <param name="guid">Guid for Lookup</param>
        /// <param name="bypassCache"> Bypass Cache flag</param>
        /// <returns>CompoundConfigurationSetting Entity</returns>
        public async Task<CompoundConfigurationSettings> GetCompoundConfigurationSettingsByGuidAsync(string guid, bool bypassCache = false)
        {
            var intgCompoundConfigurationSettings = await GetCompoundConfigurationSettingsAsync(bypassCache);
            if (intgCompoundConfigurationSettings == null || !intgCompoundConfigurationSettings.Any())
            {
                throw new KeyNotFoundException(string.Format("No compound-configuration-settings resource was found for GUID '{0}'.", guid));
            }
            var compoundSetting = intgCompoundConfigurationSettings.FirstOrDefault(cs => cs.Guid.Equals(guid, StringComparison.InvariantCultureIgnoreCase));
            if (compoundSetting == null)
            {
                return await GetCompoundConfigurationSettingsByIdAsync(await GetCompoundConfigurationSettingsIdFromGuidAsync(guid), bypassCache);
            }

            return compoundSetting;
        }

        /// <summary>
        /// Get a single Compound Configuration Settings domain entity from an compound configuration settings guid.
        /// </summary>
        /// <param name="id">CompoundConfigurationSetting Id</param>
        /// <param name="bypassCache"> Bypass Cache flag</param>
        /// <returns>CompoundConfigurationSetting Entity</returns>
        private async Task<CompoundConfigurationSettings> GetCompoundConfigurationSettingsByIdAsync(string id, bool bypassCache)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID is required to get a compound configuration settings.");
            }

            var intgCmpdSettings = await DataReader.ReadRecordAsync<IntgCmpdConfigSettings>(id);
            if (intgCmpdSettings == null)
            {
                throw new KeyNotFoundException(string.Concat("Record not found, or compound configuration settings with ID ", id, "invalid."));
            }

            var CompoundSettings = await BuildCompoundConfigurationSettingsAsync(intgCmpdSettings, bypassCache);

            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }

            return CompoundSettings;
        }


        /// <summary>
        /// Get the record key from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>Primary key</returns>
        public async Task<string> GetCompoundConfigurationSettingsIdFromGuidAsync(string guid)
        {

            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }
            var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(guid) });
            if (idDict == null || idDict.Count == 0)
            {
                throw new KeyNotFoundException(string.Format("No compound-configuration-settings was found for guid '{0}'", guid));
            }
            var foundEntry = idDict.FirstOrDefault();
            if (foundEntry.Value == null)
            {
                throw new KeyNotFoundException(string.Format("No compound-configuration-settings was found for guid '{0}'", guid));
            }

            if (foundEntry.Value.Entity != "INTG.CMPD.CONFIG.SETTINGS" || !string.IsNullOrEmpty(foundEntry.Value.SecondaryKey))
            {
                throw new KeyNotFoundException(string.Format("The GUID specified: '{0}' is used by a different entity/secondary key: {1}", guid, foundEntry.Value != null ? foundEntry.Value.Entity : string.Empty));
            }
            return foundEntry.Value.PrimaryKey;
        }

        #region Get CompoundConfigurationSettingsOptions
        /// <summary>
        /// Get a collection of IntgCmpdConfigSettings
        /// </summary>
        /// <param name="bypassCache">Bypass cache flag</param>
        /// <returns>Collection of IntgCmpdConfigSettings</returns>
        public async Task<IEnumerable<CompoundConfigurationSettingsOptions>> GetCompoundConfigurationSettingsOptionsAsync(bool bypassCache)
        {
            if (bypassCache && ContainsKey(BuildFullCacheKey("AllIntgCmpdConfigSettingsOptions")))
            {
                ClearCache(new List<string> { "AllIntgCmpdConfigSettingsOptions" });
            }
            return await GetOrAddToCacheAsync<IEnumerable<CompoundConfigurationSettingsOptions>>("AllIntgCmpdConfigSettingsOptions",
                async () =>
                {
                    var compoundSettingsIds = await DataReader.SelectAsync("INTG.CMPD.CONFIG.SETTINGS", "");
                    var compoundSettingsDataContracts = await DataReader.BulkReadRecordAsync<IntgCmpdConfigSettings>("INTG.CMPD.CONFIG.SETTINGS", compoundSettingsIds);
                    return await BuildCompoundConfigurationSettingsOptionsAsync(compoundSettingsDataContracts, bypassCache);

                }, Level1CacheTimeoutValue);
        }


        /// <summary>
        /// Get a single Compound Configuration Settings Options domain entity from an compound configuration settings options guid.
        /// </summary>
        /// <param name="guid">Guid for Lookup</param>
        /// <param name="bypassCache"> Bypass Cache flag</param>
        /// <returns>CompoundConfigurationSettingOptions Entity</returns>
        public async Task<CompoundConfigurationSettingsOptions> GetCompoundConfigurationSettingsOptionsByGuidAsync(string guid, bool bypassCache = false)
        {
            var intgCompoundConfigurationSettings = await GetCompoundConfigurationSettingsOptionsAsync(bypassCache);
            if (intgCompoundConfigurationSettings == null || !intgCompoundConfigurationSettings.Any())
            {
                throw new KeyNotFoundException(string.Format("No compound-configuration-settings resource was found for GUID '{0}'.", guid));
            }
            var compoundSetting = intgCompoundConfigurationSettings.FirstOrDefault(cs => cs.Guid.Equals(guid, StringComparison.InvariantCultureIgnoreCase));
            if (compoundSetting == null)
            {
                return await GetCompoundConfigurationSettingsOptionsByIdAsync(await GetCompoundConfigurationSettingsOptionsIdFromGuidAsync(guid), bypassCache);
            }

            return compoundSetting;
        }

        /// <summary>
        /// Get a single Compound Configuration Settings Options domain entity from an compound configuration settings options guid.
        /// </summary>
        /// <param name="id">CompoundConfigurationSettingOptions Id</param>
        /// <param name="bypassCache"> Bypass Cache flag</param>
        /// <returns>CompoundConfigurationSettingsOptions Entity</returns>
        private async Task<CompoundConfigurationSettingsOptions> GetCompoundConfigurationSettingsOptionsByIdAsync(string id, bool bypassCache)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID is required to get a compound configuration settings options.");
            }

            var intgCmpdSettings = await DataReader.ReadRecordAsync<IntgCmpdConfigSettings>(id);
            if (intgCmpdSettings == null)
            {
                throw new KeyNotFoundException(string.Concat("Record not found, or compound configuration settings with ID ", id, "invalid."));
            }

            var CompoundSettings = await BuildCompoundConfigurationSettingsOptionsAsync(intgCmpdSettings, bypassCache);

            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }

            return CompoundSettings;
        }


        /// <summary>
        /// Get the record key from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>Primary key</returns>
        public async Task<string> GetCompoundConfigurationSettingsOptionsIdFromGuidAsync(string guid)
        {

            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }
            var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(guid) });
            if (idDict == null || idDict.Count == 0)
            {
                throw new KeyNotFoundException(string.Format("No compound-configuration-settings was found for guid '{0}'", guid));
            }
            var foundEntry = idDict.FirstOrDefault();
            if (foundEntry.Value == null || foundEntry.Value.Entity != "INTG.CMPD.CONFIG.SETTINGS" || !string.IsNullOrEmpty(foundEntry.Value.SecondaryKey))
            {
                throw new KeyNotFoundException(string.Format("The GUID specified: '{0}' is used by a different entity/secondary key: {1}", guid, foundEntry.Value != null ? foundEntry.Value.Entity : string.Empty));
            }
            return foundEntry.Value.PrimaryKey;
        }
        #endregion

        #region Update CompoundConfigurationSettings

        /// <summary>
        /// Update Compound Configuration Settings 
        /// </summary>
        /// <param name="compoundSettingss">Compound Configuration Settings to update</param>     
        /// <returns>Updated Compound Configuration Settings</returns>
        public async Task<CompoundConfigurationSettings> UpdateCompoundConfigurationSettingsAsync(CompoundConfigurationSettings compoundSettingss)
        {
            if (compoundSettingss == null)
            {
                throw new ArgumentNullException("compoundSettingsEntity", "Must provide a compoundSettingsEntityEntity to update.");
            }

            if (string.IsNullOrEmpty(compoundSettingss.Guid))
            {
                throw new ArgumentNullException("compoundSettingsEntity", "Must provide the guid of the compoundSettingsEntityEntity to update.");
            }

            // verify the GUID exists to perform an update.  If not, perform a create instead
            var compoundSettingsId = await GetCompoundConfigurationSettingsIdFromGuidAsync(compoundSettingss.Guid);

            if (compoundSettingsId != null && !string.IsNullOrEmpty(compoundSettingsId))
            {
                var updateRequest = BuildUpdateRequest(compoundSettingss);

                // write the  data
                var updateResponse = await transactionInvoker.ExecuteAsync<UpdateCmpdSettingRequest, UpdateCmpdSettingResponse>(updateRequest);

                if (updateResponse.CompoundSettingsErrors.Any())
                {
                    var errorMessage = string.Empty;
                    var exception = new RepositoryException(errorMessage);
                    updateResponse.CompoundSettingsErrors.ForEach(e => exception.AddError(new RepositoryError("Validation.Exception", string.Concat(e.ErrorCodes, ": ", e.ErrorMessages))
                    {
                        Id = compoundSettingss.Guid,
                        SourceId = compoundSettingsId
                    }
                    ));
                    logger.Error(errorMessage);
                    throw exception;
                }
            }
            // get the updated entity from the database
            return await GetCompoundConfigurationSettingsByGuidAsync(compoundSettingss.Guid, true);
        }

        /// <summary>
        /// Create an UpdateCmpdSettingRequest from a domain entity
        /// </summary>
        /// <param name="compoundSettingsEntity">supporting item domain entity</param>
        /// <returns>UpdateApplicationSupportingItemRequest transaction object</returns>
        private UpdateCmpdSettingRequest BuildUpdateRequest(CompoundConfigurationSettings compoundSettingsEntity)
        {
            var request = new UpdateCmpdSettingRequest()
            {
                Guid = compoundSettingsEntity.Guid

            };
            request.CompoundConfigId = compoundSettingsEntity.Code;
            request.ConfigTitle = compoundSettingsEntity.Title;
            request.ConfigDesc = compoundSettingsEntity.Description;
            request.ConfigLabels = new List<string>();
            if (!string.IsNullOrEmpty(compoundSettingsEntity.PrimaryLabel))
                request.ConfigLabels.Add(compoundSettingsEntity.PrimaryLabel);
            if (!string.IsNullOrEmpty(compoundSettingsEntity.SecondaryLabel))
                request.ConfigLabels.Add(compoundSettingsEntity.SecondaryLabel);
            if (!string.IsNullOrEmpty(compoundSettingsEntity.TertiaryLabel))
                request.ConfigLabels.Add(compoundSettingsEntity.TertiaryLabel);
            if (compoundSettingsEntity.EthosResources != null && compoundSettingsEntity.EthosResources.Any())
            {
                foreach (var resource in compoundSettingsEntity.EthosResources)
                {
                    request.CompoundSettingsResources.Add(new CompoundSettingsResources()
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

            if (compoundSettingsEntity.Properties != null && compoundSettingsEntity.Properties.Any())
            {
                var titleValues = new List<CompoundSettingsAssociatedValues>();
                foreach (var property in compoundSettingsEntity.Properties)
                {
                    var value = new CompoundSettingsAssociatedValues();
                    value.PriTitles = property.PrimaryTitle;
                    value.PriValues = property.PrimaryValue;
                    value.SecTitles = property.SecondaryTitle;
                    value.SecValues = property.SecondaryValue;
                    value.TerTitles = property.TertiaryTitle;
                    value.TerValues = property.TertiaryValue;
                    titleValues.Add(value);
                }
                request.CompoundSettingsAssociatedValues = titleValues;
            }
            return request;
        }

        #endregion

        #region Build CompoundConfigurationSettings

        /// <summary>
        /// Build from a Collection of Data Contracts
        /// </summary>
        /// <param name="sources"></param>
        /// <returns></returns>
        private async Task<IEnumerable<CompoundConfigurationSettings>> BuildCompoundConfigurationSettingsAsync(Collection<IntgCmpdConfigSettings> sources, bool bypassCache = false)
        {
            var compoundSettingsCollection = new List<CompoundConfigurationSettings>();

            foreach (var source in sources)
            {
                compoundSettingsCollection.Add(await BuildCompoundConfigurationSettingsAsync(source, bypassCache));
            }

            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }

            return compoundSettingsCollection.AsEnumerable();
        }

        /// <summary>
        /// Build from a single data contract
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private async Task<CompoundConfigurationSettings> BuildCompoundConfigurationSettingsAsync(IntgCmpdConfigSettings source, bool bypassCache = false)
        {
            if (source == null)
            {
                exception.AddError(new RepositoryError("Bad.Data", "Selected, but can not read INTG.CMPD.CONFIG.SETTINGS record."));
                return null;
            }

            CompoundConfigurationSettings compoundSettings = null;

            try
            {
                compoundSettings = new CompoundConfigurationSettings(source.RecordGuid, source.Recordkey, source.IcmCollCmpdConfigDesc)
                {
                    Title = source.IcmCollCmpdConfigTitle,
                    EthosResources = new List<DefaultSettingsResource>(),
                    PrimaryLabel = source.IcmCollConfigLabel.ElementAtOrDefault(0),
                    SecondaryLabel = source.IcmCollConfigLabel.ElementAtOrDefault(1),
                    TertiaryLabel = source.IcmCollConfigLabel.ElementAtOrDefault(2),
                    PrimaryEntity = source.IcmCollEntity.ElementAtOrDefault(0),
                    SecondaryEntity = source.IcmCollEntity.ElementAtOrDefault(1),
                    TertiaryEntity = source.IcmCollEntity.ElementAtOrDefault(2),
                    PrimaryValcode = source.IcmCollValcodeId.ElementAtOrDefault(0),
                    SecondaryValcode = source.IcmCollValcodeId.ElementAtOrDefault(1),
                    TertiaryValcode = source.IcmCollValcodeId.ElementAtOrDefault(2)

                };
                if (source.CmpdResourcesEntityAssociation != null && source.CmpdResourcesEntityAssociation.Any())
                {
                    foreach (var resource in source.CmpdResourcesEntityAssociation)
                    {
                        if (!string.IsNullOrEmpty(resource.IcmEthosResourcesAssocMember))
                        {
                            compoundSettings.EthosResources.Add(new DefaultSettingsResource()
                            {
                                Resource = resource.IcmEthosResourcesAssocMember,
                                PropertyName = resource.IcmEthosPropertyNamesAssocMember
                            });
                        }
                    }
                }
                else
                {
                    exception.AddError(new RepositoryError("Missing.Required.Property", "Invalid INTG.CMPD.CONFIG.SETTINGS record.  Missing ethos.resource property.")
                    {
                        Id = source.RecordGuid,
                        SourceId = source.Recordkey
                    });
                }
                compoundSettings = await BuildCompoundConfigurationSettingsProperty(compoundSettings, source, bypassCache);
            }
            catch (Exception ex)
            {
                exception.AddError(new RepositoryError("Missing.Required.Property", "Invalid INTG.CMPD.CONFIG.SETTINGS record.  Missing one or more required property.  " + ex.Message)
                {
                    Id = source.RecordGuid,
                    SourceId = source.Recordkey
                });
            }

            return compoundSettings;
        }

        private async Task<CompoundConfigurationSettings> BuildCompoundConfigurationSettingsProperty(CompoundConfigurationSettings compoundSettings, IntgCmpdConfigSettings compoundSettingsDataContract, bool bypassCache = false)
        {
            if (compoundSettings == null)
            {
                throw new KeyNotFoundException("Could not find a CompoundConfigurationSettings record.");
            }

            var ldmDefaults = await GetLdmDefaults(bypassCache);
            var cmpdProperties = new List<CompoundConfigurationSettingsProperty>();
            var cmpdLabels = new List<string>();
            var title = compoundSettingsDataContract.IcmCollCmpdConfigTitle;
            switch (title.ToUpperInvariant())
            {
                case "PAYROLL DEDUCTION OCCURRENCE INTERVALS":
                    {
                        var payrollDeducts = ldmDefaults.LdmdPayPeriodEntityAssociation;
                        if (payrollDeducts != null && payrollDeducts.Any())
                            foreach (var deduction in payrollDeducts)
                            {
                                var CmpdProperty = new CompoundConfigurationSettingsProperty();
                                CmpdProperty.PrimaryValue = deduction.LdmdPayDeductIntervalAssocMember.ToString();
                                CmpdProperty.PrimaryTitle = string.Empty;
                                CmpdProperty.SecondaryValue = deduction.LdmdPayDeductPeriodAssocMember;
                                CmpdProperty.SecondaryTitle = string.Empty;
                                cmpdProperties.Add(CmpdProperty);
                            }

                        break;
                    }

                case "PAYROLL DEDUCTION MONTHLY PAY PERIODS":
                    {
                        var payrollDeducts = ldmDefaults.LdmdPayPeriodMoEntityAssociation;
                        if (payrollDeducts != null && payrollDeducts.Any())
                            foreach (var deduction in payrollDeducts)
                            {
                                var CmpdProperty = new CompoundConfigurationSettingsProperty();
                                CmpdProperty.PrimaryValue = deduction.LdmdPayDeductMonthlyAssocMember.ToString();
                                CmpdProperty.PrimaryTitle = string.Empty;
                                CmpdProperty.SecondaryValue = deduction.LdmdPayDeductMoPeriodAssocMember;
                                CmpdProperty.SecondaryTitle = string.Empty;
                                cmpdProperties.Add(CmpdProperty);
                            }

                        break;
                    }
                case "DEFAULT STUDENT PROGRAMS":
                    {
                        var programs = ldmDefaults.LdmdDefaultProgramsEntityAssociation;
                        if (programs != null && programs.Any())
                            foreach (var program in programs)
                            {
                                var CmpdProperty = new CompoundConfigurationSettingsProperty();
                                CmpdProperty.PrimaryValue = program.LdmdAcadLevelsAssocMember;
                                CmpdProperty.PrimaryTitle = await GetTitle(compoundSettings.PrimaryEntity, string.Empty, CmpdProperty.PrimaryValue, bypassCache);
                                CmpdProperty.SecondaryValue = program.LdmdAcadProgramsAssocMember;
                                CmpdProperty.SecondaryTitle = await GetTitle(compoundSettings.SecondaryEntity, string.Empty, CmpdProperty.SecondaryValue, bypassCache);
                                CmpdProperty.TertiaryValue = program.LdmdStuProgramStatusesAssocMember;
                                CmpdProperty.TertiaryTitle = await GetTitle(compoundSettings.TertiaryEntity, compoundSettings.TertiaryValcode, CmpdProperty.TertiaryValue, bypassCache);
                                cmpdProperties.Add(CmpdProperty);
                            }

                        break;
                    }
                case "COLLEAGUE SUBJECT/DEPARTMENT MAPPING":
                    {
                        var departmentsDict = await GetAllDepartmentsDescriptionAsync(bypassCache);
                        if (departmentsDict != null && departmentsDict.Any())
                        {
                            var subjectsDict = await GetAllSubjectsDescriptionAsync(bypassCache);
                            if (subjectsDict != null && subjectsDict.Any())
                            {
                                SortedDictionary<string, List<string>> sortedSubjectsDict = new SortedDictionary<string, List<string>>(subjectsDict);
                                foreach (KeyValuePair<string, List<string>> entry in sortedSubjectsDict)
                                {
                                    var CmpdProperty = new CompoundConfigurationSettingsProperty();
                                    CmpdProperty.PrimaryValue = entry.Key;
                                    //
                                    // List<string> of dict returns subj.desc and subj.intg.dept
                                    //
                                    if (entry.Value != null)
                                    {
                                        if (!string.IsNullOrEmpty(entry.Value[0]))
                                        {
                                            CmpdProperty.PrimaryTitle = entry.Value[0];
                                        }
                                        if (!string.IsNullOrEmpty(entry.Value[1]))
                                        {
                                            var deptKey = entry.Value[1];
                                            var departmentDesc = string.Empty;
                                            var departmentInfo = new List<string>();
                                            departmentsDict.TryGetValue(deptKey, out departmentInfo);
                                            if (departmentInfo != null)
                                            {
                                                departmentDesc = departmentInfo[0];
                                                if (!string.IsNullOrEmpty(departmentDesc))
                                                {
                                                    CmpdProperty.SecondaryValue = deptKey;
                                                    CmpdProperty.SecondaryTitle = departmentDesc;
                                                }
                                            }
                                        }
                                    }
                                    cmpdProperties.Add(CmpdProperty);
                                }
                            }
                        }

                        break;
                    }

            }
            compoundSettings.Properties = cmpdProperties;
            return compoundSettings;
        }


        #endregion

        #region Build CompoundConfigurationSettingsOptions

        /// <summary>
        /// Build from a Collection of Data Contracts
        /// </summary>
        /// <param name="sources"></param>
        /// <returns></returns>
        private async Task<IEnumerable<CompoundConfigurationSettingsOptions>> BuildCompoundConfigurationSettingsOptionsAsync(Collection<IntgCmpdConfigSettings> sources, bool bypassCache = false)
        {
            var compoundSettingsCollection = new List<CompoundConfigurationSettingsOptions>();

            foreach (var source in sources)
            {
                compoundSettingsCollection.Add(await BuildCompoundConfigurationSettingsOptionsAsync(source, bypassCache));
            }

            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }

            return compoundSettingsCollection.AsEnumerable();
        }

        /// <summary>
        /// Build from a single data contract
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private async Task<CompoundConfigurationSettingsOptions> BuildCompoundConfigurationSettingsOptionsAsync(IntgCmpdConfigSettings source, bool bypassCache = false)
        {
            if (source == null)
            {
                exception.AddError(new RepositoryError("Bad.Data", "Selected, but can not read INTG.CMPD.CONFIG.SETTINGS record."));
                return null;
            }

            CompoundConfigurationSettingsOptions compoundSettings = null;

            var primarySource = string.Empty;
            var secondarySource = string.Empty;
            var tertiarySource = string.Empty;
            var primarySourceCollection = new List<CompoundConfigurationSettingsOptionsSource>();
            var secondarySourceCollection = new List<CompoundConfigurationSettingsOptionsSource>();
            var tertiarySourceCollection = new List<CompoundConfigurationSettingsOptionsSource>();
                               
            var title = source.IcmCollCmpdConfigTitle;
            switch (title.ToUpperInvariant())
            {
                case "PAYROLL DEDUCTION OCCURRENCE INTERVALS":
                    {
                        for (int i = 2; i < 32; i++)
                        {
                            var primarySourceData = new CompoundConfigurationSettingsOptionsSource();
                            primarySourceData.Value = i.ToString();
                            primarySourceCollection.Add(primarySourceData);
                        }
                        secondarySource = "BD.PERIOD.CODE";
                        var periodCodes = await GetAllPeriodCodes(bypassCache);
                        if (periodCodes != null && periodCodes.Any())
                        {
                            foreach (var periodCode in periodCodes)
                            {
                                var secondarySourceData = new CompoundConfigurationSettingsOptionsSource();
                                secondarySourceData.Value = periodCode;
                                secondarySourceCollection.Add(secondarySourceData);
                            }
                        }

                        break;
                    }
                case "PAYROLL DEDUCTION MONTHLY PAY PERIODS":
                    {
                        for (int i = -1; i < 32; i++)
                        {
                            var primarySourceData = new CompoundConfigurationSettingsOptionsSource();
                            primarySourceData.Value = i.ToString();
                            primarySourceCollection.Add(primarySourceData);
                        }
                        secondarySource = "BD.PERIOD.CODE";
                        var periodCodes = await GetAllPeriodCodes(bypassCache);
                        if (periodCodes != null && periodCodes.Any())
                        {
                            foreach (var periodCode in periodCodes)
                            {
                                var secondarySourceData = new CompoundConfigurationSettingsOptionsSource();
                                secondarySourceData.Value = periodCode;
                                secondarySourceCollection.Add(secondarySourceData);
                            }
                        }
                        break;
                    }
                case "DEFAULT STUDENT PROGRAMS":
                    {
                        primarySource = "ACAD.LEVELS";
                        var acadLevelsDict = await GetAllAcademicLevelsDescriptionAsync(bypassCache);
                        if (acadLevelsDict != null && acadLevelsDict.Any())
                        {
                            SortedDictionary<string, string> sortedAcadLevelsDict = new SortedDictionary<string, string>(acadLevelsDict);
                            foreach (KeyValuePair<string, string> entry in sortedAcadLevelsDict)
                            {
                                var primarySourceData = new CompoundConfigurationSettingsOptionsSource();
                                primarySourceData.Title = entry.Value;
                                primarySourceData.Value = entry.Key;
                                primarySourceCollection.Add(primarySourceData);
                            }
                        }

                        secondarySource = "ACAD.PROGRAMS";
                        var acadProgramsDict = await GetAllAcademicProgramsDescriptionAsync(bypassCache);
                        if (acadProgramsDict != null && acadProgramsDict.Any())
                        {
                            SortedDictionary<string, string> sortedAcadProgramsDict = new SortedDictionary<string, string>(acadProgramsDict);
                            foreach (KeyValuePair<string, string> entry in sortedAcadProgramsDict)
                            {
                                var secondarySourceData = new CompoundConfigurationSettingsOptionsSource();
                                secondarySourceData.Title = entry.Value;
                                secondarySourceData.Value = entry.Key;
                                secondarySourceCollection.Add(secondarySourceData);
                            }
                        }

                        tertiarySource = "STUDENT.PROGRAM.STATUSES";
                        var studentProgramStatusesDict = await GetAllValcodeItemsAsync("ST.VALCODES", "STUDENT.PROGRAM.STATUSES", bypassCache);
                        if (studentProgramStatusesDict != null && studentProgramStatusesDict.Any())
                        {
                            SortedDictionary<string, string> sortedStudentProgramStatusesDict = new SortedDictionary<string, string>(studentProgramStatusesDict);
                            foreach (KeyValuePair<string, string> entry in sortedStudentProgramStatusesDict)
                            {
                                var tertiarySourceData = new CompoundConfigurationSettingsOptionsSource();
                                tertiarySourceData.Title = entry.Value;
                                tertiarySourceData.Value = entry.Key;
                                tertiarySourceCollection.Add(tertiarySourceData);
                            }
                        }
                        break;
                    }
                case "COLLEAGUE SUBJECT/DEPARTMENT MAPPING":
                    {
                        primarySource = "SUBJECTS";
                        var subjectsDict = await GetAllSubjectsDescriptionAsync(bypassCache);
                        if (subjectsDict != null && subjectsDict.Any())
                        {
                            SortedDictionary<string, List<string>> sortedSubjectsDict = new SortedDictionary<string, List<string>>(subjectsDict);
                            foreach (KeyValuePair<string, List<string>> entry in sortedSubjectsDict)
                            {
                                var primarySourceData = new CompoundConfigurationSettingsOptionsSource();
                                if (entry.Value != null && entry.Value.Any())
                                {
                                    primarySourceData.Title = entry.Value[0];
                                }
                                primarySourceData.Value = entry.Key;
                                primarySourceCollection.Add(primarySourceData);
                            }
                        }

                        secondarySource = "DEPTS";
                        var deptsDict = await GetAllDepartmentsDescriptionAsync(bypassCache);
                        if (deptsDict != null && deptsDict.Any())
                        {
                            SortedDictionary<string, List<string>> sortedDeptsDict = new SortedDictionary<string, List<string>>(deptsDict);
                            foreach (KeyValuePair<string, List<string>> entry in sortedDeptsDict)
                            {
                                var secondarySourceData = new CompoundConfigurationSettingsOptionsSource();
                                if (entry.Value != null && entry.Value.Any())
                                {
                                    secondarySourceData.Title = entry.Value[0];
                                    // Only allow academic departments ("A") or departments with no specific type.  DEPTS.TYPE validates against
                                    // Ellucian-maintained ADMIN.TYPES valcode table.
                                    if (entry.Value[1] == "A" || entry.Value[1] == string.Empty)
                                    {
                                        secondarySourceData.Value = entry.Key;
                                        secondarySourceCollection.Add(secondarySourceData);
                                    }
                                }
                            }
                        }
                        break;
                    }
            }
            compoundSettings = new CompoundConfigurationSettingsOptions(source.RecordGuid, source.Recordkey, source.IcmCollCmpdConfigDesc)
            {
                EthosResource = source.IcmEthosResources.FirstOrDefault(),
                EthosPropertyName = source.IcmEthosPropertyNames.FirstOrDefault(),
                PrimarySourceData = primarySourceCollection,
                SecondarySourceData = secondarySourceCollection,
                TertiarySourceData = tertiarySourceCollection
            };
            if (primarySource != string.Empty)
            {
                compoundSettings.PrimarySource = primarySource;
            }
            if (secondarySource != string.Empty)
            {
                compoundSettings.SecondarySource = secondarySource;
            }
            if (tertiarySource != string.Empty)
            {
                compoundSettings.TertiarySource = tertiarySource;
            }

            return compoundSettings;
        }
        
        #endregion

        #region Helper Methods 

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


        /// <summary>
        /// Get the title for the specific value
        /// </summary>
        /// <param name="entity">Name of the colleague entity </param>
        /// <param name="value">value to get the title of </param>
        /// <returns>Title</returns>

        private async Task<string> GetTitle(string entity, string valcode, string value, bool bypassCache)
        {
            var title = string.Empty;
            if (!string.IsNullOrEmpty(entity) && !string.IsNullOrEmpty(value))
            {
                switch (entity.ToUpperInvariant())
                {
                    case "ACAD.LEVELS":
                        {
                            title = await GetAcadLevelDescriptionAsync(value, bypassCache);
                            break;
                        }

                    case "ACAD.PROGRAMS":
                        {
                            title = await GetAcadProgramDescriptionAsync(value, bypassCache);
                            break;
                        }

                    case "ST.VALCODES":
                        {
                            if (valcode == "STUDENT.PROGRAM.STATUSES")
                            {
                                title = await GetStudentProgramStatusesDescriptionAsync(value, bypassCache);
                            }
                            break;
                        }

                }

            }
            return title;


        }

        /// <summary>
        /// Get title for the specific acad.level value
        /// </summary>
        /// <returns>Title</returns>
        private async Task<string> GetAcadLevelDescriptionAsync(string sourceValue, bool bypassCache)
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
                    throw new KeyNotFoundException(string.Format("Could not find a CompoundConfigurationSettings source title for value: '{0}'.", sourceValue));
                }
                return acadLevel.AclvDesc;
            }
            var dictAcadLevels = await GetAllAcademicLevelsDescriptionAsync(bypassCache);

            if (!dictAcadLevels.ContainsKey(sourceValue))
            {
                throw new KeyNotFoundException(string.Format("Could not find a CompoundConfigurationSettings source title for value: '{0}'.", sourceValue));
            }
            else
            {
                string sourceTitle = string.Empty;
                dictAcadLevels.TryGetValue(sourceValue, out sourceTitle);
                if (string.IsNullOrEmpty(sourceTitle))
                {
                    throw new KeyNotFoundException(string.Format("Could not find a CompoundConfigurationSettings source title for value: '{0}'.", sourceValue));
                }
                else
                {
                    return sourceTitle;
                }
            }
        }

        /// <summary>
        /// Get title for the specific acad.programs
        /// </summary>
        /// <returns>Title</returns>
        private async Task<string> GetAcadProgramDescriptionAsync(string sourceValue, bool bypassCache)
        {
            if (string.IsNullOrEmpty(sourceValue))
            {
                return string.Empty;
            }

            if (bypassCache)
            {
                var acadprogram = await DataReader.ReadRecordAsync<AcadProgramsBase>(sourceValue);
                if (acadprogram == null)
                {
                    throw new KeyNotFoundException(string.Format("Could not find a CompoundConfigurationSettings source title for value: '{0}'.", sourceValue));
                }
                return acadprogram.AcpgTitle;
            }
            var dictAcadLevels = await GetAllAcademicProgramsDescriptionAsync(bypassCache);

            if (!dictAcadLevels.ContainsKey(sourceValue))
            {
                throw new KeyNotFoundException(string.Format("Could not find a CompoundConfigurationSettings source title for value: '{0}'.", sourceValue));
            }
            else
            {
                string sourceTitle = string.Empty;
                dictAcadLevels.TryGetValue(sourceValue, out sourceTitle);
                if (string.IsNullOrEmpty(sourceTitle))
                {
                    throw new KeyNotFoundException(string.Format("Could not find a CompoundConfigurationSettings source title for value: '{0}'.", sourceValue));
                }
                else
                {
                    return sourceTitle;
                }
            }
        }


        /// <summary>
        /// Retrieve all ACAD.LEVELS records from cache and return dictionary
        /// of key and description.
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns>"Dictionary of key value pair, key and description.</returns>
        public async Task<Dictionary<string, string>> GetAllAcademicLevelsDescriptionAsync(bool bypassCache)
        {
            Dictionary<string, string> dictAcademicLevels = new Dictionary<string, string>();

            var cacheControlKey = "AllAcademicLevelsCompoundConfigurationSettings";
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
        /// Retrieve all ACAD.PROGRAMS records from cache and return dictionary
        /// of key and description.
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns>"Dictionary of key value pair, key and description.</returns>
        public async Task<Dictionary<string, string>> GetAllAcademicProgramsDescriptionAsync(bool bypassCache)
        {
            Dictionary<string, string> dictAcademicPrograms = new Dictionary<string, string>();

            var cacheControlKey = "AllAcademicProgramsCompoundConfigurationSettings";
            if (bypassCache && ContainsKey(BuildFullCacheKey(cacheControlKey)))
            {
                ClearCache(new List<string> { cacheControlKey });
            }
            var allAcadPrograms = await GetOrAddToCacheAsync<IEnumerable<AcadProgramsBase>>(cacheControlKey,
                async () =>
                {
                    var credTypes = await DataReader.BulkReadRecordAsync<AcadProgramsBase>("ACAD.PROGRAMS", "");
                    if (credTypes == null)
                    {
                        logger.Info("Unable to access ACAD.PROGRAMS from database.");
                        credTypes = new Collection<AcadProgramsBase>();
                    }
                    return credTypes;
                }, Level1CacheTimeoutValue);

            if (allAcadPrograms != null && allAcadPrograms.Any())
            {
                foreach (var acadProg in allAcadPrograms)
                {
                    if (acadProg != null && !string.IsNullOrEmpty(acadProg.AcpgTitle))
                    {
                        var dictKey = acadProg.Recordkey;
                        var dictValue = acadProg.AcpgTitle;
                        if (!string.IsNullOrEmpty(dictKey) && !dictAcademicPrograms.ContainsKey(dictKey))
                        {
                            dictAcademicPrograms.Add(dictKey, dictValue);
                        }
                    }
                }
            }

            return dictAcademicPrograms;
        }

        /// <summary>
        /// Retrieve all possible period codes (BD.PERIOD.CODE) from BENDED file.        
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns>"List of period codes.</returns>
        public async Task<List<string>> GetAllPeriodCodes(bool bypassCache)
        {
            var periodCodes = new List<string>();

            try
            {
                var cacheControlKey = "AllBendedPeriodCodes";
                if (bypassCache && ContainsKey(BuildFullCacheKey(cacheControlKey)))
                {
                    ClearCache(new List<string> { cacheControlKey });
                }
                var allBended = await GetOrAddToCacheAsync<IEnumerable<BendedBase>>(cacheControlKey,
                    async () =>
                    {
                        var bended = await DataReader.BulkReadRecordAsync<BendedBase>("BENDED", "");
                        if (bended == null)
                        {
                            logger.Info("Unable to access BENDED from database.");
                            bended = new Collection<BendedBase>();
                        }
                        return bended;
                    }, Level1CacheTimeoutValue);

                if (allBended != null && allBended.Any())
                {
                    // extract unique period codes from all BENDED
                    var allPeriodCodes = allBended.Where(b => (!string.IsNullOrWhiteSpace(b.BdPeriodCode)))
                          .Select(b => b.BdPeriodCode).Distinct().ToArray();
                    periodCodes = allPeriodCodes.ToList();
                }
            }
            catch (Exception ex)
            {
                // do nothing
            }
            return periodCodes;
        }

        /// <summary>
        /// Retrieve the title for specific value from valcode STUDENT.PROGRAM.STATUSES
        /// </summary>
        ///  <param name="sourceValue"> valcode value</param>
        /// <param name="bypassCache">cache flag</param>
        /// <returns>"Dictionary of key value pair, key and description.</returns>
        private async Task<string> GetStudentProgramStatusesDescriptionAsync(string sourceValue, bool bypassCache)
        {
            if (string.IsNullOrEmpty(sourceValue))
            {
                return string.Empty;
            }

            var dictValcodes = await GetAllValcodeItemsAsync("ST.VALCODES", "STUDENT.PROGRAM.STATUSES", bypassCache);

            if (!dictValcodes.ContainsKey(sourceValue))
            {
                throw new KeyNotFoundException(string.Format("Could not find compoundConfigurationSettings source title for value: '{0}'.", sourceValue));
            }
            else
            {
                string sourceTitle = string.Empty;
                dictValcodes.TryGetValue(sourceValue, out sourceTitle);
                if (string.IsNullOrEmpty(sourceTitle))
                {
                    throw new KeyNotFoundException(string.Format("Could not find compoundConfigurationSettings source title for value: '{0}'.", sourceValue));
                }
                else
                {
                    return sourceTitle;
                }
            }
        }

        /// <summary>
        /// Retrieve the list of valcode description & code for 
        /// </summary>
        ///  <param name="entity"> name of the valcode entity</param>
        ///  <param name="valcodeTable"> name of the valcode table</param>
        /// <param name="bypassCache">cache flag</param>
        /// <returns>"Dictionary of key value pair, key and description.</returns>    

        public async Task<Dictionary<string, string>> GetAllValcodeItemsAsync(string entity, string valcodeTable, bool bypassCache, string specialProcessing = "*")
        {
            Dictionary<string, string> dictValcodeItems = new Dictionary<string, string>();

            var cacheControlKey = string.Concat("AllValcodeTableCompoundConfigurationSettings", entity, valcodeTable, specialProcessing);
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

        /// <summary>
        /// Retrieve all SUBJECTS records from cache and return dictionary
        /// of key and description.
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns>"Dictionary of key value pair: key and description, department mapping.</returns>
        public async Task<Dictionary<string, List<string>>> GetAllSubjectsDescriptionAsync(bool bypassCache)
        {
            Dictionary<string, List<string>> dictSubjects = new Dictionary<string, List<string>>();

            var cacheControlKey = "AllSubjectsCompoundConfigurationSettings";
            if (bypassCache && ContainsKey(BuildFullCacheKey(cacheControlKey)))
            {
                ClearCache(new List<string> { cacheControlKey });
            }
            var allSubjects = await GetOrAddToCacheAsync<IEnumerable<SubjectsBase>>(cacheControlKey,
                async () =>
                {
                    var subjects = await DataReader.BulkReadRecordAsync<SubjectsBase>("SUBJECTS", "");
                    if (subjects == null)
                    {
                        logger.Info("Unable to access SUBJECTS from database.");
                        subjects = new Collection<SubjectsBase>();
                    }
                    return subjects;
                }, Level1CacheTimeoutValue);

            if (allSubjects != null && allSubjects.Any())
            {
                foreach (var subject in allSubjects)
                {
                    if (subject != null && !string.IsNullOrEmpty(subject.SubjDesc))
                    {
                        var dictKey = subject.Recordkey;
                        var dictValue = new List<string> { subject.SubjDesc, subject.SubjIntgDept };
                        if (!string.IsNullOrEmpty(dictKey) && !dictSubjects.ContainsKey(dictKey))
                        {
                            dictSubjects.Add(dictKey, dictValue);
                        }
                    }
                }
            }

            return dictSubjects;
        }

        /// <summary>
        /// Retrieve all DEPTS records from cache and return dictionary
        /// of key and description.
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns>"Dictionary of key value pair: key and description, department type.</returns>
        public async Task<Dictionary<string, List<string>>> GetAllDepartmentsDescriptionAsync(bool bypassCache)
        {
            Dictionary<string, List<string>> dictDepartments = new Dictionary<string, List<string>>();

            var cacheControlKey = "AllDepartmentsCompoundConfigurationSettings";
            if (bypassCache && ContainsKey(BuildFullCacheKey(cacheControlKey)))
            {
                ClearCache(new List<string> { cacheControlKey });
            }
            var allDepartments = await GetOrAddToCacheAsync<IEnumerable<Depts>>(cacheControlKey,
                async () =>
                {
                    var departments = await DataReader.BulkReadRecordAsync<Depts>("DEPTS", "");
                    if (departments == null)
                    {
                        logger.Info("Unable to access DEPTS from database.");
                        departments = new Collection<Depts>();
                    }
                    return departments;
                }, Level1CacheTimeoutValue);

            if (allDepartments != null && allDepartments.Any())
            {
                foreach (var department in allDepartments)
                {
                    if (department != null && !string.IsNullOrEmpty(department.DeptsDesc))
                    {
                        var dictKey = department.Recordkey;
                        var dictValue = new List<string> { department.DeptsDesc, department.DeptsType };
                        if (!string.IsNullOrEmpty(dictKey) && !dictDepartments.ContainsKey(dictKey))
                        {
                            dictDepartments.Add(dictKey, dictValue);
                        }
                    }
                }
            }

            return dictDepartments;
        }
        #endregion        
    }

}