//Copyright 2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    [RegisterType]
    public class CollectionConfigurationSettingsService : BaseCoordinationService, ICollectionConfigurationSettingsService
    {
        private readonly ICollectionConfigurationSettingsRepository _collectionConfigurationSettingsRepository;
        private readonly IReferenceDataRepository _referenceDataRepository;

        public CollectionConfigurationSettingsService(
            ICollectionConfigurationSettingsRepository collectionConfigurationSettingsRepository,
            IReferenceDataRepository referenceDataRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IConfigurationRepository configurationRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _collectionConfigurationSettingsRepository = collectionConfigurationSettingsRepository;
            _referenceDataRepository = referenceDataRepository;
        }

        #region GET collection-configuration-settings
        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all collection-configuration-settings
        /// </summary>
        /// <returns>Collection of CollectionConfigurationSettings DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.CollectionConfigurationSettings>> GetCollectionConfigurationSettingsAsync(List<DefaultSettingsEthos> resourcesFilter, bool bypassCache = false)
        {
            var collectionConfigurationSettingsCollection = new List<Dtos.CollectionConfigurationSettings>();

            var collectionConfigurationSettingsEntities = await _collectionConfigurationSettingsRepository.GetCollectionConfigurationSettingsAsync(bypassCache);
            if (resourcesFilter != null && resourcesFilter.Any())
            {
                List<Domain.Base.Entities.CollectionConfigurationSettings> newEntities = new List<Domain.Base.Entities.CollectionConfigurationSettings>();
                foreach (var entity in collectionConfigurationSettingsEntities)
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
                    return collectionConfigurationSettingsCollection;
                }
                collectionConfigurationSettingsEntities = newEntities;
            }
            if (collectionConfigurationSettingsEntities != null && collectionConfigurationSettingsEntities.Any())
            {
                foreach (var collectionConfigurationSettings in collectionConfigurationSettingsEntities)
                {
                    collectionConfigurationSettingsCollection.Add(ConvertCollectionConfigurationSettingsEntityToDto(collectionConfigurationSettings));
                }
                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }
            }
            return collectionConfigurationSettingsCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a CollectionConfigurationSettings from its GUID
        /// </summary>
        /// <returns>CollectionConfigurationSettings DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.CollectionConfigurationSettings> GetCollectionConfigurationSettingsByGuidAsync(string guid, bool bypassCache = false)
        {
            try
            {
                return ConvertCollectionConfigurationSettingsEntityToDto((await _collectionConfigurationSettingsRepository.GetCollectionConfigurationSettingsByGuidAsync(guid, bypassCache)));
            }
            catch (KeyNotFoundException ex)
            {
                throw ex;
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException(string.Format("No collection-configuration-settings was found for guid '{0}'", guid), ex);
            }
            catch (Exception ex)
            {
                throw new KeyNotFoundException(string.Format("No collection-configuration-settings was found for guid '{0}'", guid), ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a IntgCollectSettings domain entity to its corresponding CollectionConfigurationSettings DTO
        /// </summary>
        /// <param name="source">IntgCollectSettings domain entity</param>
        /// <returns>CollectionConfigurationSettings DTO</returns>
        private Ellucian.Colleague.Dtos.CollectionConfigurationSettings ConvertCollectionConfigurationSettingsEntityToDto(Domain.Base.Entities.CollectionConfigurationSettings source)
        {
            var collectionConfigurationSettings = new Dtos.CollectionConfigurationSettings()
            {
                Id = source.Guid,
                Title = source.Description
            };
            if (!string.IsNullOrEmpty(source.FieldHelp))
            {
                collectionConfigurationSettings.Description = source.FieldHelp;
            }
            if (source.Source != null && source.Source.Any())
            {
                var collectConfigSource = new List<DefaultSettingsSource>();
                foreach (var sourceValue in source.Source)
                {
                    if (!string.IsNullOrEmpty(sourceValue.SourceTitle) || !string.IsNullOrEmpty(sourceValue.SourceValue))
                    {
                        var configSourceValue = new DefaultSettingsSource()
                        {
                            Title = sourceValue.SourceTitle,
                            Value = sourceValue.SourceValue
                        };
                        collectConfigSource.Add(configSourceValue);
                    }
                }
                collectionConfigurationSettings.Source = collectConfigSource;
            }
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
                collectionConfigurationSettings.Ethos = ethosResources;
            }

            return collectionConfigurationSettings;
        }
        #endregion

        #region GET collection-configuration-settings-options
        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all collection-configuration-settings-options
        /// </summary>
        /// <returns>Collection of CollectionConfigurationSettings DTO objects</returns>
        public async Task<IEnumerable<CollectionConfigurationSettingsOptions>> GetCollectionConfigurationSettingsOptionsAsync(List<DefaultSettingsEthos> resourcesFilter, bool bypassCache = false)
        {
            var collectionConfigurationSettingsCollection = new List<CollectionConfigurationSettingsOptions>();

            var collectionConfigurationSettingsEntities = await _collectionConfigurationSettingsRepository.GetCollectionConfigurationSettingsAsync(bypassCache);
            if (resourcesFilter != null && resourcesFilter.Any())
            {
                List<Domain.Base.Entities.CollectionConfigurationSettings> newEntities = new List<Domain.Base.Entities.CollectionConfigurationSettings>();
                foreach (var entity in collectionConfigurationSettingsEntities)
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
                    return collectionConfigurationSettingsCollection;
                }
                collectionConfigurationSettingsEntities = newEntities;
            }
            if (collectionConfigurationSettingsEntities != null && collectionConfigurationSettingsEntities.Any())
            {
                foreach (var collectionConfigurationSettings in collectionConfigurationSettingsEntities)
                {
                    collectionConfigurationSettingsCollection.Add(await ConvertCollectionConfigurationSettingsOptionsEntityToDto(collectionConfigurationSettings, bypassCache));
                }
                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }
            }
            return collectionConfigurationSettingsCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a CollectionConfigurationSettingsOptions from its GUID
        /// </summary>
        /// <returns>CollectionConfigurationSettings DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.CollectionConfigurationSettingsOptions> GetCollectionConfigurationSettingsOptionsByGuidAsync(string guid, bool bypassCache = false)
        {
            try
            {
                return await ConvertCollectionConfigurationSettingsOptionsEntityToDto(await _collectionConfigurationSettingsRepository.GetCollectionConfigurationSettingsByGuidAsync(guid, bypassCache), bypassCache);
            }
            catch (KeyNotFoundException ex)
            {
                throw ex;
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException(string.Format("No collection-configuration-settings was found for guid '{0}'", guid), ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a IntgCollectSettings domain entity to its corresponding CollectionConfigurationSettings DTO
        /// </summary>
        /// <param name="source">IntgCollectSettings domain entity</param>
        /// <returns>CollectionConfigurationSettings DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.CollectionConfigurationSettingsOptions> ConvertCollectionConfigurationSettingsOptionsEntityToDto(Domain.Base.Entities.CollectionConfigurationSettings source, bool bypassCache = false)
        {
            var collectionConfigurationSettings = new Dtos.CollectionConfigurationSettingsOptions()
            {
                Id = source.Guid
            };
            collectionConfigurationSettings.SourceOptions = await GetCollectionConfigurationSettingsSourceOptions(source, bypassCache);
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
                collectionConfigurationSettings.Ethos = ethosResources;
            }

            return collectionConfigurationSettings;
        }

        private async Task<IEnumerable<DefaultSettingsSourceOptions>> GetCollectionConfigurationSettingsSourceOptions(Domain.Base.Entities.CollectionConfigurationSettings source, bool bypassCache)
        {
            List<DefaultSettingsSourceOptions> collectionConfigurationSettingsSourceOptions = new List<DefaultSettingsSourceOptions>();

            var origin = !string.IsNullOrEmpty(source.ValcodeTableName) ? source.ValcodeTableName : source.EntityName;
            if (string.IsNullOrEmpty(origin))
            {
                if (source.FieldName.Equals("LDMD.MAPPING.CONTROL", StringComparison.OrdinalIgnoreCase))
                {
                    collectionConfigurationSettingsSourceOptions.AddRange(
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
                    collectionConfigurationSettingsSourceOptions.AddRange(
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
                switch (source.EntityName)
                {
                    case "BENDED":
                        {
                            sourceDictionary = await _collectionConfigurationSettingsRepository.GetAllBendedCodesAsync(bypassCache);
                            break;
                        }
                    case "RELATION.TYPES":
                        {
                            sourceDictionary = await _collectionConfigurationSettingsRepository.GetAllRelationTypesCodesAsync(bypassCache);
                            break;
                        }
                    default:
                        {
                            if (!string.IsNullOrEmpty(source.EntityName) && !string.IsNullOrEmpty(source.ValcodeTableName))
                            {
                                sourceDictionary = await _collectionConfigurationSettingsRepository.GetAllValcodeItemsAsync(source.EntityName, source.ValcodeTableName, bypassCache);
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
                            collectionConfigurationSettingsSourceOptions.Add(new DefaultSettingsSourceOptions()
                            {
                                Title = dict.Value,
                                Value = dict.Key,
                                Origin = origin
                            });
                        }
                    }
                }
            }
            return collectionConfigurationSettingsSourceOptions;
        }
        #endregion

        /// <summary>
        /// Update a CollectionConfigurationSettings.
        /// </summary>
        /// <param name="CollectionConfigurationSettings">The <see cref="Dtos.CollectionConfigurationSettings">collectionConfigurationSettings</see> entity to update in the database.</param>
        /// <returns>The newly updated <see cref="Dtos.CollectionConfigurationSettings">collectionConfigurationSettings</see></returns>
        public async Task<Dtos.CollectionConfigurationSettings> UpdateCollectionConfigurationSettingsAsync(Dtos.CollectionConfigurationSettings collectionConfigurationSettings)
        {
            if (collectionConfigurationSettings == null)
                throw new ArgumentNullException("CollectionConfigurationSettings", "Must provide a CollectionConfigurationSettings for update");
            if (string.IsNullOrEmpty(collectionConfigurationSettings.Id))
                throw new ArgumentNullException("CollectionConfigurationSettings", "Must provide a guid for CollectionConfigurationSettings update");

            // get the ID associated with the incoming guid
            var collectionConfigurationSettingsEntityId = await _collectionConfigurationSettingsRepository.GetCollectionConfigurationSettingsIdFromGuidAsync(collectionConfigurationSettings.Id);
            if (string.IsNullOrEmpty(collectionConfigurationSettingsEntityId))
            {
                throw new ArgumentNullException("CollectionConfigurationSettings", "Must provide a guid for CollectionConfigurationSettings update");
            }
            else
            {
                // verify the user has the permission to update a collectionConfigurationSettings
                this.CheckCreateCollectionConfigurationSettingsPermission();

                _collectionConfigurationSettingsRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

                try
                {
                    // map the DTO to entities
                    var collectionConfigurationSettingsEntity
                    = await ConvertCollectionConfigurationSettingsDtoToEntityAsync(collectionConfigurationSettings);

                    // update the entity in the database
                    var updatedCollectionConfigurationSettingsEntity =
                        await _collectionConfigurationSettingsRepository.UpdateCollectionConfigurationSettingsAsync(collectionConfigurationSettingsEntity);

                    return ConvertCollectionConfigurationSettingsEntityToDto(updatedCollectionConfigurationSettingsEntity);
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
                    throw new Exception(ex.Message, ex.InnerException);
                }
            }
        }

        private async Task<Domain.Base.Entities.CollectionConfigurationSettings> ConvertCollectionConfigurationSettingsDtoToEntityAsync(Dtos.CollectionConfigurationSettings collectionConfigurationSettingsDto)
        {
            string guid = collectionConfigurationSettingsDto.Id;

            var configSettingsId = await _collectionConfigurationSettingsRepository.GetCollectionConfigurationSettingsIdFromGuidAsync(guid);
            var collectionConfigurationSettings = new Domain.Base.Entities.CollectionConfigurationSettings(collectionConfigurationSettingsDto.Id, configSettingsId, collectionConfigurationSettingsDto.Title)
            {
                FieldHelp = collectionConfigurationSettingsDto.Description,
            };

            if (collectionConfigurationSettingsDto.Ethos != null && collectionConfigurationSettingsDto.Ethos.Any())
            {
                var ethosResources = new List<DefaultSettingsResource>();
                foreach (var resource in collectionConfigurationSettingsDto.Ethos)
                {
                    ethosResources.Add(new DefaultSettingsResource()
                    {
                        Resource = resource.Resource,
                        PropertyName = resource.PropertyName
                    });
                };
                collectionConfigurationSettings.EthosResources = ethosResources;
            }

            if (collectionConfigurationSettingsDto.Source != null && collectionConfigurationSettingsDto.Source.Any())
            {
                var source = new List<CollectionConfigurationSettingsSource>();
                foreach (var setting in collectionConfigurationSettingsDto.Source)
                {
                    if (!string.IsNullOrEmpty(setting.Title) && !string.IsNullOrEmpty(setting.Value))
                    {
                        source.Add(new CollectionConfigurationSettingsSource()
                        {
                            SourceValue = setting.Value,
                            SourceTitle = setting.Title
                        });
                    }
                }
                collectionConfigurationSettings.Source = source;
            }

            return collectionConfigurationSettings;
        }

        /// <summary>
        /// Helper method to determine if the user has permission to create/update CollectionConfigurationSettings.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckCreateCollectionConfigurationSettingsPermission()
        {
            bool hasPermission = HasPermission(BasePermissionCodes.UpdateCollectionConfigurationSettings);

            // User is not allowed to create or update CollectionConfigurationSettings without the appropriate permissions
            if (!hasPermission)
            {
                throw new PermissionsException("User " + CurrentUser.UserId + " does not have permission to create/update CollectionConfigurationSettings.");
            }
        }
    }
}