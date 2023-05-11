//Copyright 2019-2022 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos;
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
    public class ConfigurationSettingsService : BaseCoordinationService, IConfigurationSettingsService
    {
        private readonly IConfigurationSettingsRepository _configurationSettingsRepository;
        private readonly IReferenceDataRepository _referenceDataRepository;

        public ConfigurationSettingsService(
            IConfigurationSettingsRepository configurationSettingsRepository,
            IReferenceDataRepository referenceDataRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IConfigurationRepository configurationRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _configurationSettingsRepository = configurationSettingsRepository;
            _referenceDataRepository = referenceDataRepository;
        }

        #region GET configuration-settings
        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all configuration-settings
        /// </summary>
        /// <returns>Collection of ConfigurationSettings DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.ConfigurationSettings>> GetConfigurationSettingsAsync(List<string> resourcesFilter, bool bypassCache = false)
        {
            var configurationSettingsCollection = new List<Ellucian.Colleague.Dtos.ConfigurationSettings>();

            var configurationSettingsEntities = await _configurationSettingsRepository.GetConfigurationSettingsAsync(bypassCache);
            if (resourcesFilter != null && resourcesFilter.Any())
            {
                List<Domain.Base.Entities.ConfigurationSettings> newEntities = new List<Domain.Base.Entities.ConfigurationSettings>();
                foreach (var resource in resourcesFilter)
                {
                    var matchingEntities = configurationSettingsEntities.Where(ce => ce.EthosResources.Contains(resource));
                    if (matchingEntities != null && matchingEntities.Any())
                    {
                        newEntities.AddRange(matchingEntities);
                    }
                }
                if (newEntities == null || !newEntities.Any())
                {
                    return configurationSettingsCollection;
                }
                configurationSettingsEntities = newEntities;
            }
            if (configurationSettingsEntities != null && configurationSettingsEntities.Any())
            {
                foreach (var configurationSettings in configurationSettingsEntities)
                {
                    configurationSettingsCollection.Add(ConvertConfigurationSettingsEntityToDto(configurationSettings));
                }
                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }
            }
            return configurationSettingsCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a ConfigurationSettings from its GUID
        /// </summary>
        /// <returns>ConfigurationSettings DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.ConfigurationSettings> GetConfigurationSettingsByGuidAsync(string guid, bool bypassCache = false)
        {
            try
            {
                return ConvertConfigurationSettingsEntityToDto((await _configurationSettingsRepository.GetConfigurationSettingsByGuidAsync(guid, bypassCache)));
            }
            catch (KeyNotFoundException ex)
            {
                throw ex;
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException(string.Format("No configuration-settings was found for guid '{0}'", guid), ex);
            }
            catch (Exception ex)
            {
                throw new KeyNotFoundException(string.Format("No configuration-settings was found for guid '{0}'", guid), ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a IntgConfigSettings domain entity to its corresponding ConfigurationSettings DTO
        /// </summary>
        /// <param name="source">IntgConfigSettings domain entity</param>
        /// <returns>ConfigurationSettings DTO</returns>
        private Ellucian.Colleague.Dtos.ConfigurationSettings ConvertConfigurationSettingsEntityToDto(Domain.Base.Entities.ConfigurationSettings source)
        {
            var configurationSettings = new Dtos.ConfigurationSettings()
            {
                Id = source.Guid,
                Title = source.Description
            };
            if (!string.IsNullOrEmpty(source.FieldHelp))
            {
                configurationSettings.Description = source.FieldHelp;
            }
            if (!string.IsNullOrEmpty(source.SourceTitle) || !string.IsNullOrEmpty(source.SourceValue))
            {
                configurationSettings.Source = new ConfigurationSettingsSource()
                {
                    Title = source.SourceTitle,
                    Value = source.SourceValue
                };
            }
            if (source.EthosResources != null && source.EthosResources.Any())
            {
                configurationSettings.Ethos = new ConfigurationSettingsEthos()
                {
                    Resources = source.EthosResources
                };
            }

            return configurationSettings;
        }
        #endregion

        #region GET configuration-settings-options
        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all configuration-settings-options
        /// </summary>
        /// <returns>Collection of ConfigurationSettings DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.ConfigurationSettingsOptions>> GetConfigurationSettingsOptionsAsync(List<string> resourcesFilter, bool bypassCache = false)
        {
            var configurationSettingsCollection = new List<Ellucian.Colleague.Dtos.ConfigurationSettingsOptions>();

            var configurationSettingsEntities = await _configurationSettingsRepository.GetConfigurationSettingsAsync(bypassCache);
            if (resourcesFilter != null && resourcesFilter.Any())
            {
                List<Domain.Base.Entities.ConfigurationSettings> newEntities = new List<Domain.Base.Entities.ConfigurationSettings>();
                foreach (var resource in resourcesFilter)
                {
                    var matchingEntities = configurationSettingsEntities.Where(ce => ce.EthosResources.Contains(resource));
                    if (matchingEntities != null && matchingEntities.Any())
                    {
                        newEntities.AddRange(matchingEntities);
                    }
                }
                if (newEntities == null || !newEntities.Any())
                {
                    return configurationSettingsCollection;
                }
                configurationSettingsEntities = newEntities;
            }
            if (configurationSettingsEntities != null && configurationSettingsEntities.Any())
            {
                foreach (var configurationSettings in configurationSettingsEntities)
                {
                    configurationSettingsCollection.Add(await ConvertConfigurationSettingsOptionsEntityToDto(configurationSettings, bypassCache));
                }
                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }
            }
            return configurationSettingsCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a ConfigurationSettingsOptions from its GUID
        /// </summary>
        /// <returns>ConfigurationSettings DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.ConfigurationSettingsOptions> GetConfigurationSettingsOptionsByGuidAsync(string guid, bool bypassCache = false)
        {
            try
            {
                return await ConvertConfigurationSettingsOptionsEntityToDto(await _configurationSettingsRepository.GetConfigurationSettingsByGuidAsync(guid, bypassCache), bypassCache);
            }
            catch (KeyNotFoundException ex)
            {
                throw ex;
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException(string.Format("No configuration-settings was found for guid '{0}'", guid), ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a IntgConfigSettings domain entity to its corresponding ConfigurationSettings DTO
        /// </summary>
        /// <param name="source">IntgConfigSettings domain entity</param>
        /// <returns>ConfigurationSettings DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.ConfigurationSettingsOptions> ConvertConfigurationSettingsOptionsEntityToDto(Domain.Base.Entities.ConfigurationSettings source, bool bypassCache = false)
        {
            var configurationSettings = new Dtos.ConfigurationSettingsOptions()
            {
                Id = source.Guid
            };
            configurationSettings.SourceOptions = await GetConfigurationSettingsSourceOptions(source, bypassCache);
            if (source.EthosResources != null && source.EthosResources.Any())
            {
                configurationSettings.Ethos = new ConfigurationSettingsEthos()
                {
                    Resources = source.EthosResources
                };
            }

            return configurationSettings;
        }

        private async Task<IEnumerable<ConfigurationSettingsSourceOptions>> GetConfigurationSettingsSourceOptions(Domain.Base.Entities.ConfigurationSettings source, bool bypassCache)
        {
            List<ConfigurationSettingsSourceOptions> configurationSettingsSourceOptions = new List<ConfigurationSettingsSourceOptions>();

            var origin = !string.IsNullOrEmpty(source.ValcodeTableName) ? source.ValcodeTableName : source.EntityName;
            if (string.IsNullOrEmpty(origin))
            {
                if (source.FieldName.Equals("LDMD.MAPPING.CONTROL", StringComparison.OrdinalIgnoreCase))
                {
                    configurationSettingsSourceOptions.AddRange(
                        new List<ConfigurationSettingsSourceOptions>()
                        {
                            new ConfigurationSettingsSourceOptions()
                            {
                                Title = "Update ethos value",
                                Value = "ethos"
                            }
                        }
                    );
                }
                else
                {
                    configurationSettingsSourceOptions.AddRange(
                        new List<ConfigurationSettingsSourceOptions>()
                        {
                        new ConfigurationSettingsSourceOptions()
                        {
                            Title = "Yes",
                            Value = "Y"
                        },
                        new ConfigurationSettingsSourceOptions()
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
                switch (source.EntityName)
                {
                    case "ELF.DUPL.CRITERIA":
                        {
                            sourceDictionary = await _configurationSettingsRepository.GetAllDuplicateCriteriaAsync(bypassCache);
                            break;
                        }
                    case "REG.USERS":
                        {
                            sourceDictionary = await _configurationSettingsRepository.GetAllRegUsersAsync(bypassCache);
                            break;
                        }
                    case "CASHIERS":
                        {
                            sourceDictionary = await _configurationSettingsRepository.GetAllCashierNamesAsync(bypassCache);
                            break;
                        }
                    default:
                        {
                            if (!string.IsNullOrEmpty(source.EntityName) && !string.IsNullOrEmpty(source.ValcodeTableName))
                            {
                                sourceDictionary = await _configurationSettingsRepository.GetAllValcodeItemsAsync(source.EntityName, source.ValcodeTableName, bypassCache);
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
                            configurationSettingsSourceOptions.Add(new ConfigurationSettingsSourceOptions()
                            {
                                Title = dict.Value,
                                Value = dict.Key,
                                Origin = origin
                            });
                        }
                    }
                }
            }
            return configurationSettingsSourceOptions;
        }
        #endregion

        /// <summary>
        /// Update a ConfigurationSettings.
        /// </summary>
        /// <param name="ConfigurationSettings">The <see cref="Dtos.ConfigurationSettings">configurationSettings</see> entity to update in the database.</param>
        /// <returns>The newly updated <see cref="Dtos.ConfigurationSettings">configurationSettings</see></returns>
        public async Task<Dtos.ConfigurationSettings> UpdateConfigurationSettingsAsync(Dtos.ConfigurationSettings configurationSettings)
        {
            if (configurationSettings == null)
                throw new ArgumentNullException("ConfigurationSettings", "Must provide a ConfigurationSettings for update");
            if (string.IsNullOrEmpty(configurationSettings.Id))
                throw new ArgumentNullException("ConfigurationSettings", "Must provide a guid for ConfigurationSettings update");

            // get the ID associated with the incoming guid
            var configurationSettingsEntityId = await _configurationSettingsRepository.GetConfigurationSettingsIdFromGuidAsync(configurationSettings.Id);
            if (string.IsNullOrEmpty(configurationSettingsEntityId))
            {
                throw new ArgumentNullException("ConfigurationSettings", "Must provide a guid for ConfigurationSettings update");
            }
            else
            {
                // verify the user has the permission to update a configurationSettings
                this.CheckCreateConfigurationSettingsPermission();

                _configurationSettingsRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

                try
                {
                    // map the DTO to entities
                    var configurationSettingsEntity
                    = await ConvertConfigurationSettingsDtoToEntityAsync(configurationSettings);

                    // update the entity in the database
                    var updatedConfigurationSettingsEntity =
                        await _configurationSettingsRepository.UpdateConfigurationSettingsAsync(configurationSettingsEntity);

                    return ConvertConfigurationSettingsEntityToDto(updatedConfigurationSettingsEntity);
                }
                catch (RepositoryException ex)
                {
                    throw ex;
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

        private async Task<Domain.Base.Entities.ConfigurationSettings> ConvertConfigurationSettingsDtoToEntityAsync(Dtos.ConfigurationSettings configurationSettingsDto)
        {
            string guid = configurationSettingsDto.Id;

            var configSettingsId = await _configurationSettingsRepository.GetConfigurationSettingsIdFromGuidAsync(guid);
            var configurationSettings = new Domain.Base.Entities.ConfigurationSettings(configurationSettingsDto.Id, configSettingsId, configurationSettingsDto.Title)
            {
                FieldHelp = configurationSettingsDto.Description,
            };

            if (configurationSettingsDto.Ethos != null && configurationSettingsDto.Ethos.Resources != null && configurationSettingsDto.Ethos.Resources.Any())
            {
                configurationSettings.EthosResources = configurationSettingsDto.Ethos.Resources;
            }

            if (configurationSettingsDto.Source != null && !string.IsNullOrEmpty(configurationSettingsDto.Source.Value))
            {
                configurationSettings.SourceValue = configurationSettingsDto.Source.Value;
                configurationSettings.SourceTitle = configurationSettingsDto.Source.Title;
            }

            return configurationSettings;
        }

        /// <summary>
        /// Helper method to determine if the user has permission to create/update ConfigurationSettings.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckCreateConfigurationSettingsPermission()
        {
            bool hasPermission = HasPermission(BasePermissionCodes.UpdateConfigurationSettings);

            // User is not allowed to create or update ConfigurationSettings without the appropriate permissions
            if (!hasPermission)
            {
                throw new PermissionsException("User " + CurrentUser.UserId + " does not have permission to create/update ConfigurationSettings.");
            }
        }
    }
}