//Copyright 2019-2022 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Coordination.Base.Adapters;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    [RegisterType]
    public class MappingSettingsService : BaseCoordinationService, IMappingSettingsService
    {

        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IMappingSettingsRepository _mappingSettingsRepository;

        public MappingSettingsService(
            IMappingSettingsRepository mappingSettingsRepository,
            IReferenceDataRepository referenceDataRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IConfigurationRepository configurationRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _mappingSettingsRepository = mappingSettingsRepository;
            _referenceDataRepository = referenceDataRepository;
        }

        #region GET mapping-settings
        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all mapping-settings
        /// </summary>
        /// <returns>Collection of MappingSettings DTO objects</returns>
        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.MappingSettings>, int>> GetMappingSettingsAsync(int offset, int limit,
            Dtos.MappingSettings criteriaFilter, bool bypassCache = false)
        {
            List<string> resources = new List<string>();
            List<string> propertyNames = new List<string>();
            if (criteriaFilter != null)
            {
                if (criteriaFilter.Ethos != null)
                {
                    if (criteriaFilter.Ethos.Resources != null && criteriaFilter.Ethos.Resources.Any())
                    {
                        foreach (var ethosResource in criteriaFilter.Ethos.Resources)
                        {
                            if (!string.IsNullOrEmpty(ethosResource.Resource))
                            {
                                resources.Add(ethosResource.Resource);
                            }
                            if (!string.IsNullOrEmpty(ethosResource.PropertyName))
                            {
                                propertyNames.Add(ethosResource.PropertyName);
                            }
                        }
                    }
                }
            }

            var mappingSettingsCollection = new List<Ellucian.Colleague.Dtos.MappingSettings>();
            try
            {
                var mappingSettingsEntities = await _mappingSettingsRepository.GetMappingSettingsAsync(offset, limit, resources, propertyNames, bypassCache);
                if (mappingSettingsEntities != null && mappingSettingsEntities.Item1.Any())
                {
                    mappingSettingsCollection = (await BuildMappingSettingsDtoAsync(mappingSettingsEntities.Item1, bypassCache)).ToList();
                }
                return mappingSettingsCollection.Any() ? new Tuple<IEnumerable<Dtos.MappingSettings>, int>(mappingSettingsCollection, mappingSettingsEntities.Item2) :
                    new Tuple<IEnumerable<Dtos.MappingSettings>, int>(new List<Dtos.MappingSettings>(), 0);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a MappingSettings from its GUID
        /// </summary>
        /// <returns>MappingSettings DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.MappingSettings> GetMappingSettingsByGuidAsync(string guid, bool bypassCache = true)
        {
            try
            {
                return await ConvertMappingSettingEntityToDtoAsync(await _mappingSettingsRepository.GetMappingSettingsByGuidAsync(guid, bypassCache));
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException(string.Format("No mapping-settings was found for guid '{0}'", guid), ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException(string.Format("No mapping-settings was found for guid '{0}'", guid), ex);
            }
        }

        /// <summary>
        /// BuildMappingSettingsDtoAsync
        /// </summary>
        /// <param name="sources">Collection of MappingSettings domain entities</param>
        /// <param name="bypassCache">bypassCache flag.  Mappinged to false</param>
        /// <returns>Collection of MappingSettings DTO objects </returns>
        private async Task<IEnumerable<Dtos.MappingSettings>> BuildMappingSettingsDtoAsync(IEnumerable<Domain.Base.Entities.MappingSettings> sources,
            bool bypassCache = false)
        {

            if ((sources == null) || (!sources.Any()))
            {
                return null;
            }
            var mappingSettings = new List<Dtos.MappingSettings>();

            foreach (var source in sources)
            {
                try
                {
                    mappingSettings.Add(await ConvertMappingSettingEntityToDtoAsync(source, bypassCache));
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, guid: source.Guid);
                }
            }

            if (IntegrationApiException != null)
                throw IntegrationApiException;

            return mappingSettings;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a MappingSettings domain entity to its corresponding MappingSettings DTO
        /// </summary>
        /// <param name="source">MappingSettings domain entity</param>
        /// <returns>MappingSettings DTO</returns>
        private async Task<Dtos.MappingSettings> ConvertMappingSettingEntityToDtoAsync(Domain.Base.Entities.MappingSettings source,
           bool bypassCache = false)
        {

            if (source == null)
                return null;

            var mappingSetting = new Ellucian.Colleague.Dtos.MappingSettings();

            mappingSetting.Id = source.Guid;
            mappingSetting.Title = source.Description;

            var ethosResource = new Ellucian.Colleague.Dtos.MappingSettingsEthosResources();
            ethosResource.Resource = source.EthosResource;
            ethosResource.PropertyName = source.EthosPropertyName;
            var ethosResources = new List<Ellucian.Colleague.Dtos.MappingSettingsEthosResources>() { ethosResource };

            var ethos = new Ellucian.Colleague.Dtos.MappingSettingsEthos();
            ethos.Resources = ethosResources;
            if (!string.IsNullOrEmpty(source.Enumeration))
            {
                ethos.Enumeration = source.Enumeration;
            }
            mappingSetting.Ethos = ethos;

            var mappingSource = new Ellucian.Colleague.Dtos.MappingSettingsSource();
            mappingSource.Value = source.SourceValue;
            mappingSource.Title = source.SourceTitle;

            mappingSetting.Source = mappingSource;

            return mappingSetting;
        }
        #endregion

        #region Get mapping-settings-options
        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all mapping-settings-options
        /// </summary>
        /// <returns>Collection of MappingSettingsOptions DTO objects</returns>
        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.MappingSettingsOptions>, int>> GetMappingSettingsOptionsAsync(int offset, int limit,
            Dtos.MappingSettingsOptions criteriaFilter, bool bypassCache = false)
        {
            List<string> resources = new List<string>();
            List<string> propertyNames = new List<string>();
            if (criteriaFilter != null)
            {
                if (criteriaFilter.Ethos != null)
                {
                    if (criteriaFilter.Ethos.Resources != null && criteriaFilter.Ethos.Resources.Any())
                    {
                        foreach (var ethosResource in criteriaFilter.Ethos.Resources)
                        {
                            if (!string.IsNullOrEmpty(ethosResource.Resource))
                            {
                                resources.Add(ethosResource.Resource);
                            }
                            if (!string.IsNullOrEmpty(ethosResource.PropertyName))
                            {
                                propertyNames.Add(ethosResource.PropertyName);
                            }
                        }
                    }
                }
            }

            var mappingSettingsOptionsCollection = new List<Ellucian.Colleague.Dtos.MappingSettingsOptions>();

            var mappingSettingsOptionsEntities = await _mappingSettingsRepository.GetMappingSettingsOptionsAsync(offset, limit, resources, propertyNames, bypassCache);
            if (mappingSettingsOptionsEntities != null && mappingSettingsOptionsEntities.Item1.Any())
            {
                mappingSettingsOptionsCollection = (await BuildMappingSettingsOptionsDtoAsync(mappingSettingsOptionsEntities.Item1, bypassCache)).ToList();
            }
            return mappingSettingsOptionsCollection.Any() ? new Tuple<IEnumerable<Dtos.MappingSettingsOptions>, int>(mappingSettingsOptionsCollection, mappingSettingsOptionsEntities.Item2) :
                new Tuple<IEnumerable<Dtos.MappingSettingsOptions>, int>(new List<Dtos.MappingSettingsOptions>(), 0);

        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a MappingSettingsOptions from its GUID
        /// </summary>
        /// <returns>MappingSettingsOptions DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.MappingSettingsOptions> GetMappingSettingsOptionsByGuidAsync(string guid, bool bypassCache = true)
        {
            return await ConvertMappingSettingOptionsEntityToDtoAsync(await _mappingSettingsRepository.GetMappingSettingsOptionsByGuidAsync(guid, bypassCache));
        }

        /// <summary>
        /// BuildMappingSettingsOptionsDtoAsync
        /// </summary>
        /// <param name="sources">Collection of MappingSettingsOptions domain entities</param>
        /// <param name="bypassCache">bypassCache flag.  Mappinged to false</param>
        /// <returns>Collection of MappingSettingsOptions DTO objects </returns>
        private async Task<IEnumerable<Dtos.MappingSettingsOptions>> BuildMappingSettingsOptionsDtoAsync(IEnumerable<Domain.Base.Entities.MappingSettingsOptions> sources,
            bool bypassCache = false)
        {

            if ((sources == null) || (!sources.Any()))
            {
                return null;
            }
            var mappingSettingsOptions = new List<Dtos.MappingSettingsOptions>();

            foreach (var source in sources)
            {
                mappingSettingsOptions.Add(await ConvertMappingSettingOptionsEntityToDtoAsync(source, bypassCache));
            }

            if (IntegrationApiException != null)
                throw IntegrationApiException;

            return mappingSettingsOptions;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a MappingSettingsOptions domain entity to its corresponding MappingSettingsOptions DTO
        /// </summary>
        /// <param name="source">MappingSettings domain entity</param>
        /// <returns>MappingSettings DTO</returns>
        private async Task<Dtos.MappingSettingsOptions> ConvertMappingSettingOptionsEntityToDtoAsync(Domain.Base.Entities.MappingSettingsOptions source,
           bool bypassCache = false)
        {

            if (source == null)
                return null;

            var mappingSettingOptions = new Ellucian.Colleague.Dtos.MappingSettingsOptions();

            mappingSettingOptions.Id = source.Guid;

            var ethosResource = new Ellucian.Colleague.Dtos.MappingSettingsEthosResources();
            ethosResource.Resource = source.EthosResource;
            ethosResource.PropertyName = source.EthosPropertyName;
            var ethosResources = new List<Ellucian.Colleague.Dtos.MappingSettingsEthosResources>() { ethosResource };

            var ethos = new Ellucian.Colleague.Dtos.MappingSettingsOptionsEthos();
            ethos.Resources = ethosResources;
            ethos.Enumerations = source.Enumerations;
            mappingSettingOptions.Ethos = ethos;

            return mappingSettingOptions;
        }
        #endregion

        public async Task<Ellucian.Colleague.Dtos.MappingSettings> UpdateMappingSettingsAsync(Ellucian.Colleague.Dtos.MappingSettings mappingSettings)
        {
            if (mappingSettings == null)
                throw new ArgumentNullException("MappingSettings", "Must provide a MappingSettings for update");
            if (string.IsNullOrEmpty(mappingSettings.Id))
                throw new ArgumentNullException("MappingSettings", "Must provide a guid for MappingSettings update");

            // get the ID associated with the incoming guid
            var mappingSettingsEntityId = await _mappingSettingsRepository.GetMappingSettingsIdFromGuidAsync(mappingSettings.Id);
            if (string.IsNullOrEmpty(mappingSettingsEntityId))
            {
                throw new ArgumentNullException("MappingSettings", "Must provide a guid for MappingSettings update");
            }
            else
            {
                // verify the user has the permission to update a mappingSettings
                this.CheckUpdateMappingSettingsPermission();

                _mappingSettingsRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

                try
                {
                    // map the DTO to entities
                    var mappingSettingsEntity
                    = await ConvertMappingSettingsDtoToEntityAsync(mappingSettings);

                    // update the entity in the database
                    var updatedMappingSettingsEntity =
                        await _mappingSettingsRepository.UpdateMappingSettingsAsync(mappingSettingsEntity);

                    return await ConvertMappingSettingEntityToDtoAsync(updatedMappingSettingsEntity);
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

        private async Task<Domain.Base.Entities.MappingSettings> ConvertMappingSettingsDtoToEntityAsync(Dtos.MappingSettings mappingSettingsDto)
        {
            var resource = string.Empty;
            var propertyName = string.Empty;
            var enumeration = string.Empty;
            var sourceValue = string.Empty;
            var sourceTitle = string.Empty;
            if (mappingSettingsDto.Ethos != null && mappingSettingsDto.Ethos.Resources != null && mappingSettingsDto.Ethos.Resources.Any())
            {
                foreach (var ethosResource in mappingSettingsDto.Ethos.Resources)
                {
                    resource = ethosResource.Resource;
                    propertyName = ethosResource.PropertyName;
                };
                enumeration = mappingSettingsDto.Ethos.Enumeration;
            }
            if (mappingSettingsDto.Source != null && !string.IsNullOrEmpty(mappingSettingsDto.Source.Value))
            {
                sourceValue = mappingSettingsDto.Source.Value;
                sourceTitle = mappingSettingsDto.Source.Title;
            }

            string guid = mappingSettingsDto.Id;
            var mappingSettingsId = await _mappingSettingsRepository.GetMappingSettingsIdFromGuidAsync(guid);
            var mappingSettings = new Domain.Base.Entities.MappingSettings(mappingSettingsDto.Id, mappingSettingsId, mappingSettingsDto.Title)
            {
                EthosResource = resource,
                EthosPropertyName = propertyName,
                SourceTitle = sourceTitle,
                SourceValue = sourceValue,
                Enumeration = enumeration,
            };
            return mappingSettings;
        }

        /// <summary>
        /// Helper method to determine if the user has permission to update MappingSettings.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckUpdateMappingSettingsPermission()
        {
            bool hasPermission = HasPermission(BasePermissionCodes.UpdateMappingSettings);

            // User is not allowed to update MappingSettings without the appropriate permissions
            if (!hasPermission)
            {
                throw new PermissionsException("User " + CurrentUser.UserId + " does not have permission to update MappingSettings.");
            }
        }
    }
}
