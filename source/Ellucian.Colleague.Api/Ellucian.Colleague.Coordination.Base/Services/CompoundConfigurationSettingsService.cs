//Copyright 2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    [RegisterType]
    public class CompoundConfigurationSettingsService : BaseCoordinationService, ICompoundConfigurationSettingsService
    {

        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly ICompoundConfigurationSettingsRepository _compoundSettingsRepository;


        public CompoundConfigurationSettingsService(

            IReferenceDataRepository referenceDataRepository,
            ICompoundConfigurationSettingsRepository compoundSettingsRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IConfigurationRepository configurationRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _compoundSettingsRepository = compoundSettingsRepository;
            _referenceDataRepository = referenceDataRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all compound-configuration-settings
        /// </summary>
        /// <returns>Collection of CompoundConfigurationSettings DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.CompoundConfigurationSettings>> GetCompoundConfigurationSettingsAsync(List<Dtos.DtoProperties.CompoundConfigurationSettingsEthos> resourcesFilter, bool bypassCache = false)
        {
            var compoundConfigurationSettingsCollection = new List<Ellucian.Colleague.Dtos.CompoundConfigurationSettings>();

            var compoundConfigurationSettingsEntities = await _compoundSettingsRepository.GetCompoundConfigurationSettingsAsync(bypassCache);

            if (resourcesFilter != null && resourcesFilter.Any())
            {
                List<Domain.Base.Entities.CompoundConfigurationSettings> filteredEntities = new List<Domain.Base.Entities.CompoundConfigurationSettings>();
                foreach (var entity in compoundConfigurationSettingsEntities)
                {
                    bool match = true;
                    foreach (var resource in resourcesFilter)
                    {

                        if (!string.IsNullOrEmpty(resource.Resource) && !string.IsNullOrEmpty(resource.PropertyName))
                        {
                            var matchingResources = entity.EthosResources.Where(ce => ce.Resource.Equals(resource.Resource, StringComparison.InvariantCultureIgnoreCase) && ce.PropertyName.Equals(resource.PropertyName, StringComparison.InvariantCultureIgnoreCase));
                            if (matchingResources == null || !matchingResources.Any())
                            {
                                match = false;
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(resource.Resource))
                            {
                                var matchingResources = entity.EthosResources.Where(ce => ce.Resource.Equals(resource.Resource, StringComparison.InvariantCultureIgnoreCase));
                                if (matchingResources == null || !matchingResources.Any())
                                {
                                    match = false;
                                }
                            }
                            if (!string.IsNullOrEmpty(resource.PropertyName))
                            {
                                var matchingPropertyNames = entity.EthosResources.Where(ce => ce.PropertyName.Equals(resource.PropertyName, StringComparison.InvariantCultureIgnoreCase));
                                if (matchingPropertyNames == null || !matchingPropertyNames.Any())
                                {
                                    match = false;
                                }
                            }
                        }
                    }
                    if (match)
                    {
                        filteredEntities.Add(entity);
                    }
                }
                if (filteredEntities == null || !filteredEntities.Any())
                {
                    return compoundConfigurationSettingsCollection;
                }
                compoundConfigurationSettingsEntities = filteredEntities;
            }
            if (compoundConfigurationSettingsEntities != null && compoundConfigurationSettingsEntities.Any())
            {
                foreach (var compoundConfigurationSettings in compoundConfigurationSettingsEntities)
                {
                    compoundConfigurationSettingsCollection.Add(ConvertCompoundConfigurationSettingsEntityToDto(compoundConfigurationSettings));
                }
                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }
            }
            return compoundConfigurationSettingsCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a CompoundConfigurationSettings from its GUID
        /// </summary>
        /// <returns>CompoundConfigurationSettings DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.CompoundConfigurationSettings> GetCompoundConfigurationSettingsByGuidAsync(string guid, bool bypassCache = true)
        {
            try
            {
                return ConvertCompoundConfigurationSettingsEntityToDto(await _compoundSettingsRepository.GetCompoundConfigurationSettingsByGuidAsync(guid, bypassCache));
            }
            catch (KeyNotFoundException ex)
            {
                throw ex;
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException(string.Format("No compound-configuration-settings was found for guid '{0}'", guid), ex);
            }
        }

        #region GET compound-configuration-settings-options
        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all compound-configuration-settings-options
        /// </summary>
        /// <returns>Collection of CompoundConfigurationSettingsOptions DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.CompoundConfigurationSettingsOptions>> GetCompoundConfigurationSettingsOptionsAsync
            (List<Dtos.DtoProperties.CompoundConfigurationSettingsEthos> resourcesFilter, bool bypassCache = false)
        {
            var compoundConfigurationSettingsOptionsCollection = new List<Ellucian.Colleague.Dtos.CompoundConfigurationSettingsOptions>();

            var compoundConfigurationSettingsOptionsEntities = await _compoundSettingsRepository.GetCompoundConfigurationSettingsOptionsAsync(bypassCache);

            if (resourcesFilter != null && resourcesFilter.Any())
            {
                List<Domain.Base.Entities.CompoundConfigurationSettingsOptions> filteredEntities = new List<Domain.Base.Entities.CompoundConfigurationSettingsOptions>();

                foreach (var entity in compoundConfigurationSettingsOptionsEntities)
                {
                    bool matches = true;
                    foreach (var resource in resourcesFilter)
                    {
                        if (!string.IsNullOrEmpty(resource.Resource))
                        {
                            if (resource.Resource != entity.EthosResource)
                            {
                                matches = false;
                            }
                        }
                        if (!string.IsNullOrEmpty(resource.PropertyName))
                        {
                            if (resource.PropertyName != entity.EthosPropertyName)
                            {
                                matches = false;
                            }
                        }
                    }
                    if (matches)
                    {
                        filteredEntities.Add(entity);
                    }
                }
                if (filteredEntities == null || !filteredEntities.Any())
                {
                    return compoundConfigurationSettingsOptionsCollection;
                }
                compoundConfigurationSettingsOptionsEntities = filteredEntities;
            }

            if (compoundConfigurationSettingsOptionsEntities != null && compoundConfigurationSettingsOptionsEntities.Any())
            {
                foreach (var compoundConfigurationSettingsOptions in compoundConfigurationSettingsOptionsEntities)
                {
                    compoundConfigurationSettingsOptionsCollection.Add(ConvertCompoundConfigurationSettingsOptionsEntityToDto(compoundConfigurationSettingsOptions));
                }
                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }
            }
            return compoundConfigurationSettingsOptionsCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a CompoundConfigurationSettingsOptions from its GUID
        /// </summary>
        /// <returns>CompoundConfigurationSettingsOptions DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.CompoundConfigurationSettingsOptions> GetCompoundConfigurationSettingsOptionsByGuidAsync(string guid, bool bypassCache = false)
        {
            try
            {
                return ConvertCompoundConfigurationSettingsOptionsEntityToDto(await _compoundSettingsRepository.GetCompoundConfigurationSettingsOptionsByGuidAsync(guid, bypassCache));
            }
            catch (KeyNotFoundException ex)
            {
                throw ex;
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException(string.Format("No compound-configuration-settings-options was found for guid '{0}'", guid), ex);
            }

        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a CompoundConfigurationSettingsOptions domain entity to its corresponding CompoundConfigurationSettingsOptions DTO
        /// </summary>
        /// <param name="source">CompoundConfigurationSettingsOptions domain entity</param>
        /// <returns>CompoundConfigurationSettingsOptions DTO</returns>
        private Ellucian.Colleague.Dtos.CompoundConfigurationSettingsOptions ConvertCompoundConfigurationSettingsOptionsEntityToDto(Domain.Base.Entities.CompoundConfigurationSettingsOptions source)
        {
            var compoundConfigurationSettingsOptions = new Ellucian.Colleague.Dtos.CompoundConfigurationSettingsOptions();

            compoundConfigurationSettingsOptions.Id = source.Guid;

            var ethos = new CompoundConfigurationSettingsEthos()
            {
                Resource = source.EthosResource,
                PropertyName = source.EthosPropertyName
            };
            compoundConfigurationSettingsOptions.Ethos = new List<CompoundConfigurationSettingsEthos>() { ethos };
            var ethosSource = new Dtos.DtoProperties.CompoundConfigurationSettingsOptionsSource();

            var primarySourceValue = new CompoundConfigurationSettingsOptionsPrimaryValue();
            if (source.PrimarySource != null)
            {
                primarySourceValue.Origin = source.PrimarySource;
            }
            if (source.PrimarySourceData != null && source.PrimarySourceData.Any())
            {
                var sourceOptions = new List<CompoundConfigurationSettingsSourceOptions>();
                foreach (var sourceData in source.PrimarySourceData)
                {
                    var sourceOption = new CompoundConfigurationSettingsSourceOptions();
                    sourceOption.Title = sourceData.Title;
                    sourceOption.Value = sourceData.Value;
                    sourceOptions.Add(sourceOption);
                }
                primarySourceValue.SourceOptions = sourceOptions;
                ethosSource.PrimaryValue = primarySourceValue;
            }

            var secondarySourceValue = new CompoundConfigurationSettingsOptionsSecondaryValue();
            if (source.SecondarySource != null)
            {
                secondarySourceValue.Origin = source.SecondarySource;
            }
            if (source.SecondarySourceData != null && source.SecondarySourceData.Any())
            {
                var sourceOptions = new List<CompoundConfigurationSettingsSourceOptions>();
                foreach (var sourceData in source.SecondarySourceData)
                {
                    var sourceOption = new CompoundConfigurationSettingsSourceOptions();
                    sourceOption.Title = sourceData.Title;
                    sourceOption.Value = sourceData.Value;
                    sourceOptions.Add(sourceOption);
                }
                secondarySourceValue.SourceOptions = sourceOptions;
                ethosSource.SecondaryValue = secondarySourceValue;
            }

            var tertiarySourceValue = new CompoundConfigurationSettingsOptionsTertiaryValue();
            if (source.TertiarySource != null)
            {
                tertiarySourceValue.Origin = source.TertiarySource;
            }
            if (source.TertiarySourceData != null && source.TertiarySourceData.Any())
            {
                var sourceOptions = new List<CompoundConfigurationSettingsSourceOptions>();
                foreach (var sourceData in source.TertiarySourceData)
                {
                    var sourceOption = new CompoundConfigurationSettingsSourceOptions();
                    sourceOption.Title = sourceData.Title;
                    sourceOption.Value = sourceData.Value;
                    sourceOptions.Add(sourceOption);
                }
                tertiarySourceValue.SourceOptions = sourceOptions;
                ethosSource.TertiaryValue = tertiarySourceValue;
            }
            compoundConfigurationSettingsOptions.Source = ethosSource;
            return compoundConfigurationSettingsOptions;
        }
        #endregion

        /// <summary>
        /// Update a CompoundConfigurationSettings.
        /// </summary>
        /// <param name="CompoundConfigurationSettings">The <see cref="CompoundConfigurationSettings">compoundConfigurationSettings</see> entity to update in the database.</param>
        /// <returns>The newly updated <see cref="CompoundConfigurationSettings">compoundConfigurationSettings</see></returns>
        public async Task<Dtos.CompoundConfigurationSettings> UpdateCompoundConfigurationSettingsAsync(Dtos.CompoundConfigurationSettings compoundConfigurationSettings)
        {
            if (compoundConfigurationSettings == null)
                throw new ArgumentNullException("CompoundConfigurationSettings", "Must provide a CompoundConfigurationSettings for update");
            if (string.IsNullOrEmpty(compoundConfigurationSettings.Id))
                throw new ArgumentNullException("CompoundConfigurationSettings", "Must provide a guid for CompoundConfigurationSettings update");

            // get the ID associated with the incoming guid
            var compoundConfigurationSettingsEntityId = await _compoundSettingsRepository.GetCompoundConfigurationSettingsIdFromGuidAsync(compoundConfigurationSettings.Id);
            if (!string.IsNullOrEmpty(compoundConfigurationSettingsEntityId))
            {
                // verify the user has the permission to update a compoundConfigurationSettings
                this.CheckCreateCompoundConfigurationSettingsPermission();

                _compoundSettingsRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

                try
                {
                    // map the DTO to entities
                    var compoundConfigurationSettingsEntity
                    = ConvertCompoundConfigurationSettingsDtoToEntityAsync(compoundConfigurationSettingsEntityId, compoundConfigurationSettings);

                    // update the entity in the database
                    var updatedCompoundConfigurationSettingsEntity =
                        await _compoundSettingsRepository.UpdateCompoundConfigurationSettingsAsync(compoundConfigurationSettingsEntity);

                    return ConvertCompoundConfigurationSettingsEntityToDto(updatedCompoundConfigurationSettingsEntity);
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
                catch (IntegrationApiException ex)
                {
                    throw ex;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message, ex.InnerException);
                }
            }
            else
            {
                throw new ArgumentNullException("CompoundConfigurationSettings", "Must provide a valid guid for CompoundConfigurationSettings update");
            }

        }

        /// <summary>
        /// Converts CompoundConfigurationSettings DTO to Entity
        /// </summary>
        /// <param name="compoundConfigurationSettingsId">Id</param>
        /// <param name="compoundConfigurationSettingsDto">compoundConfigurationSettings DTO</param>
        /// <returns>CompoundConfigurationSettings Entity</returns>
        private Domain.Base.Entities.CompoundConfigurationSettings ConvertCompoundConfigurationSettingsDtoToEntityAsync(string compoundConfigurationSettingsId, Dtos.CompoundConfigurationSettings compoundConfigurationSettingsDto)
        {
            if (string.IsNullOrEmpty(compoundConfigurationSettingsDto.Title))
            {
                IntegrationApiExceptionAddError("Title is a required property.", "Missing.Required.Property", compoundConfigurationSettingsDto.Id, compoundConfigurationSettingsId);
            }
            if (string.IsNullOrEmpty(compoundConfigurationSettingsDto.Description))
            {
                IntegrationApiExceptionAddError("The description cannot be changed for a compound configuration setting.", "Validation.Exception", compoundConfigurationSettingsDto.Id, compoundConfigurationSettingsId);
            }

            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }

            var compoundSettingEntity = new Domain.Base.Entities.CompoundConfigurationSettings(compoundConfigurationSettingsDto.Id, compoundConfigurationSettingsId, compoundConfigurationSettingsDto.Description)
            {
                Title = compoundConfigurationSettingsDto.Title
            };
            if (compoundConfigurationSettingsDto.Ethos != null && compoundConfigurationSettingsDto.Ethos.Any())
            {
                var ethosResources = new List<DefaultSettingsResource>();
                foreach (var resource in compoundConfigurationSettingsDto.Ethos)
                {
                    var ethosResource = new DefaultSettingsResource();
                    ethosResource.Resource = resource.Resource;
                    ethosResource.PropertyName = resource.PropertyName;
                    ethosResources.Add(ethosResource);
                }
                compoundSettingEntity.EthosResources = ethosResources;
            }
            else
            {
                IntegrationApiExceptionAddError("Ethos is a required property.", "Missing.Required.Property", compoundConfigurationSettingsDto.Id, compoundConfigurationSettingsId);
            }

            if (compoundConfigurationSettingsDto.Source != null)
            {
                compoundSettingEntity.PrimaryLabel = compoundConfigurationSettingsDto.Source.PrimaryDisplayLabel;
                compoundSettingEntity.SecondaryLabel = compoundConfigurationSettingsDto.Source.SecondaryDisplayLabel;
                compoundSettingEntity.TertiaryLabel = compoundConfigurationSettingsDto.Source.TertiaryDisplayLabel;

                if (compoundConfigurationSettingsDto.Source.AssociatedSettings != null && compoundConfigurationSettingsDto.Source.AssociatedSettings.Any() && compoundConfigurationSettingsDto.Source.PairedSettings != null && compoundConfigurationSettingsDto.Source.PairedSettings.Any())
                {
                    IntegrationApiExceptionAddError("Either source.pairedSettings or source.associatedSettings is allowed. Not both.", "Validation.Exception", compoundConfigurationSettingsDto.Id, compoundConfigurationSettingsId);
                }
                else
                {
                    if (compoundConfigurationSettingsDto.Source.AssociatedSettings != null && compoundConfigurationSettingsDto.Source.AssociatedSettings.Any())
                    {
                        var properties = new List<CompoundConfigurationSettingsProperty>();
                        foreach (var setting in compoundConfigurationSettingsDto.Source.AssociatedSettings)
                        {
                            if (string.IsNullOrEmpty(setting.PrimaryValue))
                            {
                                IntegrationApiExceptionAddError("Source.associatedSettings.primaryValue is required.", "Missing.Required.Property", compoundConfigurationSettingsDto.Id, compoundConfigurationSettingsId);
                            }
                            else
                            {
                                if ((!string.IsNullOrEmpty(setting.SecondaryTitle) && string.IsNullOrEmpty(setting.SecondaryValue)) || (!string.IsNullOrEmpty(setting.TertiaryTitle) && string.IsNullOrEmpty(setting.TertiaryValue)))
                                {
                                    IntegrationApiExceptionAddError("Source.associatedSettings Title is not valid for a null value.", "Validation.Exception", compoundConfigurationSettingsDto.Id, compoundConfigurationSettingsId);
                                }
                                else
                                {
                                    var property = new CompoundConfigurationSettingsProperty();
                                    property.PrimaryTitle = setting.PrimaryTitle;
                                    property.PrimaryValue = setting.PrimaryValue;
                                    property.SecondaryTitle = setting.SecondaryTitle;
                                    property.SecondaryValue = setting.SecondaryValue;
                                    property.TertiaryTitle = setting.TertiaryTitle;
                                    property.TertiaryValue = setting.TertiaryValue;
                                    properties.Add(property);
                                }
                            }
                        }
                        compoundSettingEntity.Properties = properties;
                    }
                    //else if (compoundConfigurationSettingsDto.Source.PairedSettings != null && compoundConfigurationSettingsDto.Source.PairedSettings.Any())
                    if (compoundConfigurationSettingsDto.Source.PairedSettings != null && compoundConfigurationSettingsDto.Source.PairedSettings.Any())
                    {
                        if (!string.IsNullOrEmpty(compoundConfigurationSettingsDto.Source.TertiaryDisplayLabel))
                        {
                            IntegrationApiExceptionAddError("Source.tertiaryDisplayLabel is not valid for the source with pairedSettings.", "Validation.Exception", compoundConfigurationSettingsDto.Id, compoundConfigurationSettingsId);
                        }
                        var properties = new List<CompoundConfigurationSettingsProperty>();
                        foreach (var setting in compoundConfigurationSettingsDto.Source.PairedSettings)
                        {
                            if (string.IsNullOrEmpty(setting.PrimaryValue))
                            {
                                IntegrationApiExceptionAddError("Source.pairedSettings.primaryValue is required.", "Missing.Required.Property", compoundConfigurationSettingsDto.Id, compoundConfigurationSettingsId);
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(setting.SecondaryTitle) && string.IsNullOrEmpty(setting.SecondaryValue))
                                {
                                    IntegrationApiExceptionAddError("Source.pairedSettings.SecondaryTitle is not valid for empty SecondaryValue.", "Validation.Exception", compoundConfigurationSettingsDto.Id, compoundConfigurationSettingsId);
                                }
                                else
                                {
                                    var property = new CompoundConfigurationSettingsProperty();
                                    property.PrimaryTitle = setting.PrimaryTitle;
                                    property.PrimaryValue = setting.PrimaryValue;
                                    property.SecondaryTitle = setting.SecondaryTitle;
                                    property.SecondaryValue = setting.SecondaryValue;
                                    properties.Add(property);
                                }
                            }
                        }
                        compoundSettingEntity.Properties = properties;
                    }
                    //else
                    //{
                    //    //one of the setting is required.if there is tertiary label then associatedSetting is required otherwise pairedSetting is required.
                    //    if (!string.IsNullOrEmpty(compoundConfigurationSettingsDto.Source.TertiaryDisplayLabel))
                    //    {
                    //        IntegrationApiExceptionAddError("Source.associatedSettings is required.", "Missing.Required.Property", compoundConfigurationSettingsDto.Id, compoundConfigurationSettingsId);
                    //    }
                    //    else
                    //    {
                    //        IntegrationApiExceptionAddError("Source.pairedSettings is required.", "Missing.Required.Property", compoundConfigurationSettingsDto.Id, compoundConfigurationSettingsId);
                    //    }

                    //}
                }
            }
            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }

            return compoundSettingEntity;


        }

        /// <summary>
        /// Helper method to determine if the user has permission to create/update CompoundConfigurationSettings.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckCreateCompoundConfigurationSettingsPermission()
        {
            bool hasPermission = HasPermission(BasePermissionCodes.UpdateCompoundConfigurationSettings);

            // User is not allowed update CompoundConfigurationSettings without the appropriate permissions
            if (!hasPermission)
            {
                throw new PermissionsException("User " + CurrentUser.UserId + " does not have permission to update compound-configuration-settings.");
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a CompoundConfigurationSettings domain entity to its corresponding CompoundConfigurationSettings DTO
        /// </summary>
        /// <param name="source">CompoundConfigurationSettings domain entity</param>
        /// <returns>CompoundConfigurationSettings DTO</returns>
        private Ellucian.Colleague.Dtos.CompoundConfigurationSettings ConvertCompoundConfigurationSettingsEntityToDto(Domain.Base.Entities.CompoundConfigurationSettings source)
        {
            var compoundConfigurationSettings = new Ellucian.Colleague.Dtos.CompoundConfigurationSettings();

            compoundConfigurationSettings.Id = source.Guid;
            compoundConfigurationSettings.Description = source.Description;
            compoundConfigurationSettings.Title = source.Title;
            if (source.EthosResources != null && source.EthosResources.Any())
            {
                var ethosResources = new List<CompoundConfigurationSettingsEthos>();
                foreach (var resource in source.EthosResources)
                {
                    var ethosResource = new CompoundConfigurationSettingsEthos();
                    ethosResource.Resource = resource.Resource;
                    ethosResource.PropertyName = string.IsNullOrEmpty(resource.PropertyName) ? null : resource.PropertyName;
                    ethosResources.Add(ethosResource);
                }
                compoundConfigurationSettings.Ethos = ethosResources;
            }
            //if there is tertiary label, we are going to use CompoundConfigurationSettingsAssociatedsettings dto, otherwise we use CompoundConfigurationSettingsPairedSettings
            if (!string.IsNullOrEmpty(source.TertiaryLabel))
            {
                var associatedSetting = new CompoundConfigurationSettingsSource();
                associatedSetting.PrimaryDisplayLabel = source.PrimaryLabel;
                associatedSetting.SecondaryDisplayLabel = source.SecondaryLabel;
                associatedSetting.TertiaryDisplayLabel = source.TertiaryLabel;
                if (source.Properties != null && source.Properties.Any())
                {
                    var settings = new List<CompoundConfigurationSettingsAssociatedSettings>();
                    foreach (var property in source.Properties)
                    {
                        var setting = new CompoundConfigurationSettingsAssociatedSettings();
                        if (!string.IsNullOrEmpty(property.PrimaryTitle))
                        {
                            setting.PrimaryTitle = property.PrimaryTitle;
                        }
                        setting.PrimaryValue = property.PrimaryValue;
                        if (!string.IsNullOrEmpty(property.SecondaryTitle))
                        {
                            setting.SecondaryTitle = property.SecondaryTitle;
                        }
                        if (!string.IsNullOrEmpty(property.SecondaryValue))
                        {
                            setting.SecondaryValue = property.SecondaryValue;
                        }
                        if (!string.IsNullOrEmpty(property.TertiaryTitle))
                        {
                            setting.TertiaryTitle = property.TertiaryTitle;
                        }
                        if (!string.IsNullOrEmpty(property.TertiaryValue))
                        {
                            setting.TertiaryValue = property.TertiaryValue;
                        }
                        settings.Add(setting);
                    }
                    associatedSetting.AssociatedSettings = settings;
                }
                compoundConfigurationSettings.Source = associatedSetting;
            }
            else
            {
                var pairedSetting = new CompoundConfigurationSettingsSource();
                pairedSetting.PrimaryDisplayLabel = source.PrimaryLabel;
                pairedSetting.SecondaryDisplayLabel = source.SecondaryLabel;
                if (source.Properties != null && source.Properties.Any())
                {
                    var settings = new List<CompoundConfigurationSettingsPairedSettings>();
                    foreach (var property in source.Properties)
                    {
                        var setting = new CompoundConfigurationSettingsPairedSettings();
                        if (!string.IsNullOrEmpty(property.PrimaryTitle))
                        {
                            setting.PrimaryTitle = property.PrimaryTitle;
                        }
                        setting.PrimaryValue = property.PrimaryValue;
                        if (!string.IsNullOrEmpty(property.SecondaryTitle))
                        {
                            setting.SecondaryTitle = property.SecondaryTitle;
                        }
                        if (!string.IsNullOrEmpty(property.SecondaryValue))
                        {
                            setting.SecondaryValue = property.SecondaryValue;
                        }
                        settings.Add(setting);
                    }
                    pairedSetting.PairedSettings = settings;
                }
                compoundConfigurationSettings.Source = pairedSetting;
            }
            return compoundConfigurationSettings;
        }
    }
}
