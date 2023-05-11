//Copyright 2019-2022 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    [RegisterType]
    public class DefaultSettingsService : BaseCoordinationService, IDefaultSettingsService
    {
        private readonly IDefaultSettingsRepository _defaultSettingsRepository;
        private readonly IReferenceDataRepository _referenceDataRepository;

        public DefaultSettingsService(
            IDefaultSettingsRepository defaultSettingsRepository,
            IReferenceDataRepository referenceDataRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IConfigurationRepository configurationRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _defaultSettingsRepository = defaultSettingsRepository;
            _referenceDataRepository = referenceDataRepository;
        }

        #region GET default-settings
        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all default-settings
        /// </summary>
        /// <returns>Collection of DefaultSettings DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.DefaultSettings>> GetDefaultSettingsAsync(List<DefaultSettingsEthos> resourcesFilter, bool bypassCache = false)
        {
            var defaultSettingsCollection = new List<Ellucian.Colleague.Dtos.DefaultSettings>();

            var defaultSettingsEntities = await _defaultSettingsRepository.GetDefaultSettingsAsync(bypassCache);
            if (resourcesFilter != null && resourcesFilter.Any())
            {
                List<Domain.Base.Entities.DefaultSettings> newEntities = new List<Domain.Base.Entities.DefaultSettings>();
                foreach (var entity in defaultSettingsEntities)
                {
                    bool matches = true;
                    foreach (var resource in resourcesFilter)
                    {
                        if (!string.IsNullOrEmpty(resource.Resource) && !string.IsNullOrEmpty(resource.PropertyName))
                        {
                            var matchingResources = entity.EthosResources.Where(ce => ce.Resource.Equals(resource.Resource, StringComparison.InvariantCultureIgnoreCase) && ce.PropertyName.Equals(resource.PropertyName, StringComparison.InvariantCultureIgnoreCase));
                            if (matchingResources == null || !matchingResources.Any())
                            {
                                matches = false;
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(resource.Resource))
                            {
                                var matchingResources = entity.EthosResources.Where(ce => ce.Resource.Equals(resource.Resource, StringComparison.InvariantCultureIgnoreCase));
                                if (matchingResources == null || !matchingResources.Any())
                                {
                                    matches = false;
                                }
                            }
                            if (!string.IsNullOrEmpty(resource.PropertyName))
                            {
                                var matchingPropertyNames = entity.EthosResources.Where(ce => ce.PropertyName.Equals(resource.PropertyName, StringComparison.InvariantCultureIgnoreCase));
                                if (matchingPropertyNames == null || !matchingPropertyNames.Any())
                                {
                                    matches = false;
                                }
                            }
                        }
                    }
                    if (matches)
                    {
                        newEntities.Add(entity);
                    }
                }
                if (newEntities == null || !newEntities.Any())
                {
                    return defaultSettingsCollection;
                }
                defaultSettingsEntities = newEntities;
            }
            if (defaultSettingsEntities != null && defaultSettingsEntities.Any())
            {
                foreach (var defaultSettings in defaultSettingsEntities)
                {
                    defaultSettingsCollection.Add(ConvertDefaultSettingsEntityToDto(defaultSettings));
                }
                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }
            }
            return defaultSettingsCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a DefaultSettings from its GUID
        /// </summary>
        /// <returns>DefaultSettings DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.DefaultSettings> GetDefaultSettingsByGuidAsync(string guid, bool bypassCache = false)
        {
            try
            {
                return ConvertDefaultSettingsEntityToDto((await _defaultSettingsRepository.GetDefaultSettingsByGuidAsync(guid, bypassCache)));
            }
            catch (KeyNotFoundException ex)
            {
                throw ex;
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException(string.Format("No default-settings was found for guid '{0}'", guid), ex);
            }
            catch (Exception ex)
            {
                throw new KeyNotFoundException(string.Format("No default-settings was found for guid '{0}'", guid), ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a IntgConfigSettings domain entity to its corresponding DefaultSettings DTO
        /// </summary>
        /// <param name="source">IntgConfigSettings domain entity</param>
        /// <returns>DefaultSettings DTO</returns>
        private Ellucian.Colleague.Dtos.DefaultSettings ConvertDefaultSettingsEntityToDto(Domain.Base.Entities.DefaultSettings source)
        {
            var defaultSettings = new Dtos.DefaultSettings()
            {
                Id = source.Guid,
                Title = source.Description
            };
            if (!string.IsNullOrEmpty(source.FieldHelp))
            {
                defaultSettings.Description = source.FieldHelp;
            }
            if (!string.IsNullOrEmpty(source.SourceTitle) || !string.IsNullOrEmpty(source.SourceValue))
            {
                defaultSettings.Source = new DefaultSettingsSource()
                {
                    Title = source.SourceTitle,
                    Value = source.SourceValue
                };
            }
            if (source.EthosResources != null && source.EthosResources.Any())
            {
                var ethosResources = new List<DefaultSettingsEthos>();
                foreach(var resource in source.EthosResources)
                {
                    var ethos = new DefaultSettingsEthos()
                    {
                        Resource = resource.Resource
                    };
                    if (!string.IsNullOrEmpty(resource.PropertyName)) ethos.PropertyName = resource.PropertyName;
                    ethosResources.Add(ethos);
                };
                defaultSettings.Ethos = ethosResources;
            }

            DefaultAdvancedSearchProperty advSearch = null;
            if (!string.IsNullOrEmpty(source.SearchType) && source.SearchMinLength.HasValue)
            {
                advSearch = new Dtos.DtoProperties.DefaultAdvancedSearchProperty();
                advSearch.MinSearchLength = source.SearchMinLength.Value;
                if (source.SearchType.Equals("A", StringComparison.OrdinalIgnoreCase))
                {
                    advSearch.AdvanceSearchType = Dtos.EnumProperties.AdvanceSearchType.AdvancedSearchAvailable;
                }
                else  if(source.SearchType.Equals("R", StringComparison.OrdinalIgnoreCase))
                {
                    advSearch.AdvanceSearchType = Dtos.EnumProperties.AdvanceSearchType.AdvancedSearchRequired;
                }
            }
            defaultSettings.AdvancedSearch = advSearch;

            return defaultSettings;
        }
        #endregion

        #region GET default-settings-options
        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all default-settings-options
        /// </summary>
        /// <returns>Collection of DefaultSettings DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.DefaultSettingsOptions>> GetDefaultSettingsOptionsAsync(List<DefaultSettingsEthos> resourcesFilter, bool bypassCache = false)
        {
            var defaultSettingsCollection = new List<Ellucian.Colleague.Dtos.DefaultSettingsOptions>();

            var defaultSettingsEntities = await _defaultSettingsRepository.GetDefaultSettingsAsync(bypassCache);
            if (resourcesFilter != null && resourcesFilter.Any())
            {
                List<Domain.Base.Entities.DefaultSettings> newEntities = new List<Domain.Base.Entities.DefaultSettings>();
                foreach (var entity in defaultSettingsEntities)
                {
                    bool matches = true;
                    foreach (var resource in resourcesFilter)
                    {
                        if (!string.IsNullOrEmpty(resource.Resource) && !string.IsNullOrEmpty(resource.PropertyName))
                        {
                            var matchingResources = entity.EthosResources.Where(ce => ce.Resource.Equals(resource.Resource, StringComparison.InvariantCultureIgnoreCase) && ce.PropertyName.Equals(resource.PropertyName, StringComparison.InvariantCultureIgnoreCase));
                            if (matchingResources == null || !matchingResources.Any())
                            {
                                matches = false;
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(resource.Resource))
                            {
                                var matchingResources = entity.EthosResources.Where(ce => ce.Resource.Equals(resource.Resource, StringComparison.InvariantCultureIgnoreCase));
                                if (matchingResources == null || !matchingResources.Any())
                                {
                                    matches = false;
                                }
                            }
                            if (!string.IsNullOrEmpty(resource.PropertyName))
                            {
                                var matchingPropertyNames = entity.EthosResources.Where(ce => ce.PropertyName.Equals(resource.PropertyName, StringComparison.InvariantCultureIgnoreCase));
                                if (matchingPropertyNames == null || !matchingPropertyNames.Any())
                                {
                                    matches = false;
                                }
                            }
                        }
                    }
                    if (matches)
                    {
                        newEntities.Add(entity);
                    }
                }
                if (newEntities == null || !newEntities.Any())
                {
                    return defaultSettingsCollection;
                }
                defaultSettingsEntities = newEntities;
            }
            if (defaultSettingsEntities != null && defaultSettingsEntities.Any())
            {
                foreach (var defaultSettings in defaultSettingsEntities)
                {
                    defaultSettingsCollection.Add(await ConvertDefaultSettingsOptionsEntityToDto(defaultSettings, bypassCache));
                }
                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }
            }
            return defaultSettingsCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a DefaultSettingsOptions from its GUID
        /// </summary>
        /// <returns>DefaultSettings DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.DefaultSettingsOptions> GetDefaultSettingsOptionsByGuidAsync(string guid, bool bypassCache = false)
        {
            try
            {
                return await ConvertDefaultSettingsOptionsEntityToDto(await _defaultSettingsRepository.GetDefaultSettingsByGuidAsync(guid, bypassCache), bypassCache);
            }
            catch (KeyNotFoundException ex)
            {
                throw ex;
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException(string.Format("No default-settings was found for guid '{0}'", guid), ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a IntgConfigSettings domain entity to its corresponding DefaultSettings DTO
        /// </summary>
        /// <param name="source">IntgConfigSettings domain entity</param>
        /// <returns>DefaultSettings DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.DefaultSettingsOptions> ConvertDefaultSettingsOptionsEntityToDto(Domain.Base.Entities.DefaultSettings source, bool bypassCache = false)
        {
            var defaultSettings = new Dtos.DefaultSettingsOptions()
            {
                Id = source.Guid
            };
            defaultSettings.SourceOptions = await GetDefaultSettingsSourceOptions(source, bypassCache);

            if (source.EthosResources != null && source.EthosResources.Any())
            {
                var ethosResources = new List<DefaultSettingsEthos>();
                foreach (var resource in source.EthosResources)
                {
                    var ethos = new DefaultSettingsEthos()
                    {
                        Resource = resource.Resource
                    };
                    if (!string.IsNullOrEmpty(resource.PropertyName)) ethos.PropertyName = resource.PropertyName;
                    ethosResources.Add(ethos);
                };
                defaultSettings.Ethos = ethosResources;
            }

            return defaultSettings;
        }

        private async Task<IEnumerable<DefaultSettingsSourceOptions>> GetDefaultSettingsSourceOptions(Domain.Base.Entities.DefaultSettings source, bool bypassCache)
        {
            List<DefaultSettingsSourceOptions> defaultSettingsSourceOptions = new List<DefaultSettingsSourceOptions>();

            var origin = !string.IsNullOrEmpty(source.ValcodeTableName) ? source.ValcodeTableName : source.EntityName;
            if (string.IsNullOrEmpty(origin))
            {
                if (source.FieldName.Equals("LDMD.MAPPING.CONTROL", StringComparison.OrdinalIgnoreCase))
                {
                    defaultSettingsSourceOptions.AddRange(
                        new List<DefaultSettingsSourceOptions>()
                        {
                            new DefaultSettingsSourceOptions()
                            {
                                Title = "Update ethos value",
                                Value = "ethos"
                            }
                        }
                    );
                }
                else
                {
                    defaultSettingsSourceOptions.AddRange(
                        new List<DefaultSettingsSourceOptions>()
                        {
                        new DefaultSettingsSourceOptions()
                        {
                            Title = "Yes",
                            Value = "Y"
                        },
                        new DefaultSettingsSourceOptions()
                        {
                            Title = "No",
                            Value = "N"
                        }
                        }
                    );
                }
            }
            else
            {
                Dictionary<string, string> sourceDictionary = new Dictionary<string, string>();
                var entityNameValues = source.EntityName.Split(',');
                var entityName = entityNameValues[0];

                switch (entityName.ToUpperInvariant())
                {
                    case "AR.CODES":
                        {
                            sourceDictionary = await _defaultSettingsRepository.GetAllArCodesAsync(bypassCache);
                            break;
                        }
                    case "AR.TYPES":
                        {
                            sourceDictionary = await _defaultSettingsRepository.GetAllArTypesAsync(bypassCache);
                            break;
                        }
                    case "AP.TYPES":
                        {
                            sourceDictionary = await _defaultSettingsRepository.GetAllApTypesAsync(bypassCache);
                            break;
                        }
                    case "CRED.TYPES":
                        {
                            sourceDictionary = await _defaultSettingsRepository.GetAllCreditTypesAsync(bypassCache);
                            break;
                        }
                    case "ACAD.LEVELS":
                        {
                            sourceDictionary = await _defaultSettingsRepository.GetAllAcademicLevelsAsync(bypassCache);
                            break;
                        }
                    case "ASGMT.CONTRACT.TYPES":
                        {
                            sourceDictionary = await _defaultSettingsRepository.GetAllAssignmentContractTypesAsync(bypassCache);
                            break;
                        }
                    case "POSITION":
                        {
                            sourceDictionary = await _defaultSettingsRepository.GetAllPositionsAsync(bypassCache);
                            break;
                        }
                    case "LOAD.PERIODS":
                        {
                            sourceDictionary = await _defaultSettingsRepository.GetAllLoadPeriodsAsync(bypassCache);
                            break;
                        }
                    case "BENDED":
                        {
                            sourceDictionary = await _defaultSettingsRepository.GetAllBendedCodesAsync(bypassCache);
                            break;
                        }
                    case "APPLICATION.STATUSES":
                        {
                            sourceDictionary = await _defaultSettingsRepository.GetAllApplicationStatusesAsync(bypassCache);
                            break;
                        }
                    case "RCPT.TENDER.GL.DISTR":
                        {
                            sourceDictionary = await _defaultSettingsRepository.GetAllReceiptTenderGlDistrsAsync(bypassCache);
                            break;
                        }
                    case "PAYMENT.METHOD":
                        {
                            sourceDictionary = await _defaultSettingsRepository.GetAllPaymentMethodsAsync(bypassCache);
                            break;
                        }
                    case "CORP.FOUNDS":
                        {
                            var fieldNames = source.FieldName.Split(',');
                            if (fieldNames[0] == "LDMD.COLL.FIELD.NAME")
                            {
                                sourceDictionary = await _defaultSettingsRepository.GetAllApprovalAgenciesAsync(bypassCache);
                            }
                            break;
                        }
                    case "STAFF":
                        {
                            var fieldNames = source.FieldName.Split(',');
                            if (fieldNames[0] == "LDMD.COLL.FIELD.NAME")
                            {
                                sourceDictionary = await _defaultSettingsRepository.GetAllStaffApprovalsAsync(bypassCache);
                            }
                            else
                            {
                                sourceDictionary = await _defaultSettingsRepository.GetAllApplicationStaffAsync(bypassCache);
                            }
                            break;
                        }
                    case "PERSON":
                        {
                            sourceDictionary = await _defaultSettingsRepository.GetAllSponsorsAsync(bypassCache);
                            break;
                        }
                    default:
                        {
                            if (!string.IsNullOrEmpty(source.EntityName) && !string.IsNullOrEmpty(source.ValcodeTableName))
                            {
                                string specialProcessing = "*";
                                var fieldNames = source.FieldName.Split(',');
                                if (fieldNames[0] == "LDMD.COURSE.ACT.STATUS") specialProcessing = "1";
                                if (fieldNames[0] == "LDMD.COURSE.INACT.STATUS") specialProcessing = "2";
                                sourceDictionary = await _defaultSettingsRepository.GetAllValcodeItemsAsync(source.EntityName, source.ValcodeTableName, bypassCache, specialProcessing);
                            }
                            break;
                        }
                }

                if (sourceDictionary != null && sourceDictionary.Any())
                {
                    foreach (var dict in sourceDictionary)
                    {
                        if (!string.IsNullOrEmpty(dict.Key) && !string.IsNullOrEmpty(dict.Value))
                        {
                            defaultSettingsSourceOptions.Add(new DefaultSettingsSourceOptions()
                            {
                                Title = dict.Value,
                                Value = dict.Key,
                                Origin = origin
                            });
                        }
                    }
                }
            }
            return defaultSettingsSourceOptions;
        }
        #endregion

        #region GET default-settings-advanced-search-options
        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all default-settings-options
        /// </summary>
        /// <returns>Collection of DefaultSettings DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.DefaultSettingsAdvancedSearchOptions>> GetDefaultSettingsAdvancedSearchOptionsAsync(Ellucian.Colleague.Dtos.Filters.DefaultSettingsFilter advancedSearchFilter, bool bypassCache = false)
        {
            if (advancedSearchFilter == null)
            {
                throw new ArgumentNullException("DefaultSettings", "advancedSearch must be provided");
            }
            else
            {
                if (string.IsNullOrEmpty(advancedSearchFilter.Keyword))
                {
                    throw new ArgumentNullException("DefaultSettings", "keyword must be provided for advancedSearch");
                }
                if (advancedSearchFilter.DefaultSettings == null)
                {
                    throw new ArgumentNullException("DefaultSettings", "defaultSettings must be provided for advancedSearch");
                }
                else
                {
                    if (string.IsNullOrEmpty(advancedSearchFilter.DefaultSettings.Id))
                    {
                        throw new ArgumentNullException("DefaultSettings", "defaultSettings.id must be provided for advancedSearch");
                    }
                }
            }

            var defaultSettingsCollection = new List<Dtos.DefaultSettingsAdvancedSearchOptions>();   
            // Get the ID associated with the incoming guid.
            var defaultSettingsEntityId = string.Empty;
            try
            {
                defaultSettingsEntityId = await _defaultSettingsRepository.GetDefaultSettingsIdFromGuidAsync(advancedSearchFilter.DefaultSettings.Id);
            }
            catch
            {
                throw new ArgumentNullException("DefaultSettings", "No default-settings was found for GUID " + advancedSearchFilter.DefaultSettings.Id + ".");
            }

            try
            {
                var entity = await _defaultSettingsRepository.GetDefaultSettingsAdvancedSearchOptionsAsync(
                defaultSettingsEntityId, advancedSearchFilter.Keyword, bypassCache);
                if (entity != null && entity.DefaultSettingsAdvancedSearchOptions != null && entity.DefaultSettingsAdvancedSearchOptions.Any())
                {
                    foreach (var option in entity.DefaultSettingsAdvancedSearchOptions)
                    {
                        var defaultSettingsOption = new Dtos.DefaultSettingsAdvancedSearchOptions();
                        defaultSettingsOption.Title = option.Title;
                        defaultSettingsOption.Value = option.Value;
                        defaultSettingsOption.Origin = option.Origin;
                        defaultSettingsCollection.Add(defaultSettingsOption);
                    }
                }
            }
            catch (Exception ex)
            {
                // throw any repo errors encountered by CTX call
                throw ex;
            }

            return defaultSettingsCollection;
            
        }
        #endregion

        /// <summary>
        /// Update a DefaultSettings.
        /// </summary>
        /// <param name="DefaultSettings">The <see cref="Dtos.DefaultSettings">defaultSettings</see> entity to update in the database.</param>
        /// <returns>The newly updated <see cref="Dtos.DefaultSettings">defaultSettings</see></returns>
        public async Task<Dtos.DefaultSettings> UpdateDefaultSettingsAsync(Dtos.DefaultSettings defaultSettings)
        {
            if (defaultSettings == null)
                throw new ArgumentNullException("DefaultSettings", "Must provide a DefaultSettings for update");
            if (string.IsNullOrEmpty(defaultSettings.Id))
                throw new ArgumentNullException("DefaultSettings", "Must provide a guid for DefaultSettings update");

            // get the ID associated with the incoming guid
            var defaultSettingsEntityId = await _defaultSettingsRepository.GetDefaultSettingsIdFromGuidAsync(defaultSettings.Id);
            if (string.IsNullOrEmpty(defaultSettingsEntityId))
            {
                throw new ArgumentNullException("DefaultSettings", "Must provide a guid for DefaultSettings update");
            }
            else
            {
                // verify the user has the permission to update a defaultSettings
                this.CheckCreateDefaultSettingsPermission();

                _defaultSettingsRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

                try
                {
                    // map the DTO to entities
                    var defaultSettingsEntity
                    = await ConvertDefaultSettingsDtoToEntityAsync(defaultSettings);

                    // update the entity in the database
                    var updatedDefaultSettingsEntity =
                        await _defaultSettingsRepository.UpdateDefaultSettingsAsync(defaultSettingsEntity);

                    return ConvertDefaultSettingsEntityToDto(updatedDefaultSettingsEntity);
                }
                catch (RepositoryException ex)
                {
                    IntegrationApiExceptionAddError(ex);
                    throw IntegrationApiException;
                }
                catch (KeyNotFoundException ex)
                {
                    throw ex;
                }
                catch (ArgumentException ex)
                {
                    throw ex;
                }
                catch (Exception ex)
                {
                    throw new ColleagueWebApiException(ex.Message, ex.InnerException);
                }
            }
        }

        private async Task<Domain.Base.Entities.DefaultSettings> ConvertDefaultSettingsDtoToEntityAsync(Dtos.DefaultSettings defaultSettingsDto)
        {
            string guid = defaultSettingsDto.Id;

            var defaultSettingsId = await _defaultSettingsRepository.GetDefaultSettingsIdFromGuidAsync(guid);
            var defaultSettings = new Domain.Base.Entities.DefaultSettings(defaultSettingsDto.Id, defaultSettingsId, defaultSettingsDto.Title)
            {
                FieldHelp = defaultSettingsDto.Description,
            };

            if (defaultSettingsDto.Ethos != null && defaultSettingsDto.Ethos.Any())
            {
                var ethosResources = new List<DefaultSettingsResource>();
                foreach (var resource in defaultSettingsDto.Ethos)
                {
                    ethosResources.Add(new DefaultSettingsResource()
                    {
                        Resource = resource.Resource,
                        PropertyName = resource.PropertyName
                    });
                };
                defaultSettings.EthosResources = ethosResources;
            }

            if (defaultSettingsDto.Source != null && !string.IsNullOrEmpty(defaultSettingsDto.Source.Value))
            {
                defaultSettings.SourceValue = defaultSettingsDto.Source.Value;
                defaultSettings.SourceTitle = defaultSettingsDto.Source.Title;
            }
            
            if(defaultSettingsDto.AdvancedSearch != null)
            {
                defaultSettings.SearchMinLength = defaultSettingsDto.AdvancedSearch.MinSearchLength;
                
                if(defaultSettingsDto.AdvancedSearch.AdvanceSearchType == Dtos.EnumProperties.AdvanceSearchType.AdvancedSearchAvailable)
                {
                    defaultSettings.SearchType = "A";
                }
                else if (defaultSettingsDto.AdvancedSearch.AdvanceSearchType == Dtos.EnumProperties.AdvanceSearchType.AdvancedSearchRequired)
                {
                    defaultSettings.SearchType = "R";
                }
            }

            return defaultSettings;
        }

        /// <summary>
        /// Helper method to determine if the user has permission to create/update DefaultSettings.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckCreateDefaultSettingsPermission()
        {
            bool hasPermission = HasPermission(BasePermissionCodes.UpdateDefaultSettings);

            // User is not allowed to create or update DefaultSettings without the appropriate permissions
            if (!hasPermission)
            {
                throw new PermissionsException("User " + CurrentUser.UserId + " does not have permission to update default-settings.");
            }
        }
    }
}