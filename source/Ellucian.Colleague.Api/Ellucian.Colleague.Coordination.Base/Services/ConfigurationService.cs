// Copyright 2014-2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Adapters;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using Ellucian.Web.Mvc.Install;
using Ellucian.Web.Mvc.Install.Backup;
using Ellucian.Web.Resource;
using Ellucian.Web.Security;
using Newtonsoft.Json;
using slf4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    [RegisterType]
    public class ConfigurationService : BaseCoordinationService, IConfigurationService
    {
        private readonly IConfigurationRepository configurationRepository;
        private readonly ILogger logger;
        private readonly string colleagueTimeZone;
        private readonly ApiSettings apiSettings;
        private IApiSettingsRepository apiSettingsRepository;
        private ISettingsRepository xmlSettingsRepository;
        private IResourceRepository resourceRepository;
        private IReferenceDataRepository referenceDataRepository;
        private string environmentName;

        private const string CreateApiBackupConfigPermission = "CREATE.API.BACKUP.CONFIGURATION";
        private const string RestoreApiConfigPermission = "RESTORE.API.CONFIGURATION";
        private const string CreateBackupConfigPermission = "CREATE.BACKUP.CONFIGURATION";
        private const string ViewBackupConfigPermission = "VIEW.BACKUP.CONFIGURATION";

        /// <summary>
        /// Initializes a new instance of the <see cref="FacilitiesService"/> class.
        /// </summary>
        /// <param name="configurationRepository">The configuration repository.</param>
        /// <param name="adapterRegistry">The adapter registry.</param>
        /// <param name="currentUserFactory">The current user factory.</param>
        /// <param name="roleRepository">The role repository.</param>
        /// <param name="logger">The logger.</param>
        public ConfigurationService(IConfigurationRepository configurationRepository, IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository,
            ApiSettings settings, ISettingsRepository xmlSettingsRepository,
            IApiSettingsRepository apiSettingsRepository,
            IResourceRepository resourceRepository, IReferenceDataRepository referenceDataRepository, ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            this.configurationRepository = configurationRepository;
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }
            this.logger = logger;

            colleagueTimeZone = settings.ColleagueTimeZone;
            apiSettings = settings;
            this.apiSettingsRepository = apiSettingsRepository;
            this.xmlSettingsRepository = xmlSettingsRepository;
            this.resourceRepository = resourceRepository;
            this.referenceDataRepository = referenceDataRepository;

            var xmlSettings = this.xmlSettingsRepository.Get();
            environmentName = "";
            if (settings != null)
            {
                environmentName = '/' + xmlSettings.ColleagueSettings.DmiSettings.AccountName;
            }

        }

        /// <summary>
        /// Gets an integration configuration
        /// </summary>
        /// <param name="id">Integration Configuration ID</param>
        /// <returns>An integration configuration</returns>
        public async Task<Dtos.Base.IntegrationConfiguration> GetIntegrationConfiguration(string id)
        {
            CheckIntegrationConfigPermission();
            var configEntity = await configurationRepository.GetIntegrationConfiguration(id);
            var integrationConfigurationAdapter = new IntegrationConfigurationEntityAdapter(_adapterRegistry, base.logger);
            return integrationConfigurationAdapter.MapToType(configEntity);
        }

        /// <summary>
        /// Helper method to determine if the user has permission to view integration configuration information.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        protected void CheckIntegrationConfigPermission()
        {
            bool hasConfigPermission = HasPermission(BasePermissionCodes.ViewIntegrationConfig);

            // User is not allowed to view integration configuration information without the appropriate permissions
            if (!hasConfigPermission)
            {
                var message = "User " + CurrentUser.UserId + " does not have permission to view integration configuration information.";
                base.logger.Info(message);
                throw new PermissionsException(message);
            }
        }

        /// <summary>
        /// Calls configuration repository and returns the User Profile Configuration data transfer object.
        /// </summary>
        /// <returns><see cref="Dtos.Base.UserProfileConfiguration">User Profile Configuration</see> data transfer object.</returns>
        [Obsolete("Obsolete as of API 1.16. Use version 2 instead.")]
        public async Task<Dtos.Base.UserProfileConfiguration> GetUserProfileConfigurationAsync()
        {
            var configuration = await configurationRepository.GetUserProfileConfigurationAsync();
            var adapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.UserProfileConfiguration, Dtos.Base.UserProfileConfiguration>();
            var configurationDto = adapter.MapToType(configuration);
            return configurationDto;
        }

        /// <summary>
        /// Calls configuration repository and returns the User Profile Configuration data transfer object.
        /// </summary>
        /// <returns><see cref="Dtos.Base.UserProfileConfiguration2">User Profile Configuration</see> data transfer object.</returns>
        public async Task<Dtos.Base.UserProfileConfiguration2> GetUserProfileConfiguration2Async()
        {
            // Retrieve all ADREL.TYPE codes.
            var allAdrelTypes = referenceDataRepository.AddressRelationTypes;
            var configuration = await configurationRepository.GetUserProfileConfiguration2Async(allAdrelTypes.ToList());
            var adapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.UserProfileConfiguration2, Dtos.Base.UserProfileConfiguration2>();
            var configurationDto = adapter.MapToType(configuration);
            return configurationDto;
        }

        /// <summary>
        /// Calls configuration repository and returns the Emergency Information Configuration data transfer object.
        /// </summary>
        /// <returns><see cref="Dtos.Base.EmergencyInformationConfiguration">Emergency Information Configuration</see> data transfer object.</returns>
        [Obsolete("Obsolete as of API 1.16. Use GetEmergencyInformationConfiguration2Async instead.")]
        public async Task<Dtos.Base.EmergencyInformationConfiguration> GetEmergencyInformationConfigurationAsync()
        {
            var configuration = await configurationRepository.GetEmergencyInformationConfigurationAsync();
            var adapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.EmergencyInformationConfiguration, Dtos.Base.EmergencyInformationConfiguration>();
            var configurationDto = adapter.MapToType(configuration);
            return configurationDto;
        }

        /// <summary>
        /// Calls configuration repository and returns the Emergency Information Configuration data transfer object.
        /// </summary>
        /// <returns><see cref="EmergencyInformationConfiguration2">Emergency Information Configuration</see> data transfer object.</returns>
        public async Task<Dtos.Base.EmergencyInformationConfiguration2> GetEmergencyInformationConfiguration2Async()
        {
            var configuration = await configurationRepository.GetEmergencyInformationConfigurationAsync();
            var adapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.EmergencyInformationConfiguration, Dtos.Base.EmergencyInformationConfiguration2>();
            var configurationDto = adapter.MapToType(configuration);
            return configurationDto;
        }

        /// <summary>
        /// Calls configuration repository and returns the Restriction Configuration data transfer object
        /// </summary>
        /// <returns><see cref="RestrictionConfiguration">Restriction Configuration</see></returns>
        public async Task<Dtos.Base.RestrictionConfiguration> GetRestrictionConfigurationAsync()
        {
            var configuration = await configurationRepository.GetRestrictionConfigurationAsync();
            var adapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.RestrictionConfiguration, Dtos.Base.RestrictionConfiguration>();
            var configurationDto = adapter.MapToType(configuration);
            return configurationDto;
        }

        /// <summary>
        /// Returns the privacy configuration
        /// </summary>
        /// <returns>Privacy Configuration DTO</returns>
        public async Task<Dtos.Base.PrivacyConfiguration> GetPrivacyConfigurationAsync()
        {
            var configuration = await configurationRepository.GetPrivacyConfigurationAsync();
            var adapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.PrivacyConfiguration, Dtos.Base.PrivacyConfiguration>();
            var configurationDto = adapter.MapToType(configuration);
            return configurationDto;
        }

        /// <summary>
        /// Retrieves the organizational relationship configuration
        /// </summary>
        /// <returns>Organizational relationship configuration DTO</returns>
        public async Task<Dtos.Base.OrganizationalRelationshipConfiguration> GetOrganizationalRelationshipConfigurationAsync()
        {
            var configuration = await configurationRepository.GetOrganizationalRelationshipConfigurationAsync();
            var adapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.OrganizationalRelationshipConfiguration, Dtos.Base.OrganizationalRelationshipConfiguration>();
            var configurationDto = adapter.MapToType(configuration);
            return configurationDto;
        }


        #region backup configuration


        /// <summary>
        /// Stores the supplied backup config data to the Colleague DB
        /// </summary>
        /// <param name="backupConfiguration"></param>
        /// <returns></returns>
        public async Task<Ellucian.Colleague.Dtos.Base.BackupConfiguration> WriteBackupConfigurationAsync(
            Ellucian.Colleague.Dtos.Base.BackupConfiguration backupConfiguration)
        {
            // verify permission
            if (!HasPermission(CreateBackupConfigPermission))
            {
                base.logger.Error("User does not have permission" + CreateBackupConfigPermission + " to create backup configuration records.");
                throw new PermissionsException("User does not have permission to create backup configuration records.");
            }

            if (backupConfiguration == null)
            {
                throw new ArgumentNullException("backupConfiguration");
            }

            // convert backupconfigdata dto to entity
            var backupConfigEntityAdapter = _adapterRegistry.GetAdapter<
                Ellucian.Colleague.Dtos.Base.BackupConfiguration,
                Ellucian.Colleague.Domain.Base.Entities.BackupConfiguration>();
            var backupConfigDataEntity = backupConfigEntityAdapter.MapToType(backupConfiguration);

            var result = await configurationRepository.AddBackupConfigurationAsync(backupConfigDataEntity);

            // convert entity to dto
            var backupConfigDtoAdapter = _adapterRegistry.GetAdapter<
                Ellucian.Colleague.Domain.Base.Entities.BackupConfiguration,
                Ellucian.Colleague.Dtos.Base.BackupConfiguration>();
            var backupConfigDataDto = backupConfigDtoAdapter.MapToType(result);
            return backupConfigDataDto;

        }

        /// <summary>
        /// Retrieve the latest backup config data of the given namespace from the Colleague DB. An optional
        /// date time filter can be used.
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Base.BackupConfiguration>> ReadBackupConfigurationAsync(BackupConfigurationQueryCriteria criteria)
        {
            // verify permission
            if (!HasPermission(ViewBackupConfigPermission))
            {
                base.logger.Error("User does not have permission" + ViewBackupConfigPermission + " to retrieve backup configuration records.");
                throw new PermissionsException("User does not have permission to retrieve backup configuration records.");
            }

            if (criteria == null)
            {
                throw new ArgumentNullException("criteria");
            }

            if ((criteria.ConfigurationIds == null || criteria.ConfigurationIds.Count() == 0) && string.IsNullOrEmpty(criteria.Namespace))
            {
                throw new ArgumentException("Either Configuration ID or nameSpace must be specified.");
            }

            IEnumerable<Domain.Base.Entities.BackupConfiguration> entityResultSet = null;
            if (criteria.ConfigurationIds != null && criteria.ConfigurationIds.Count() > 0)
            {
                entityResultSet = await configurationRepository.GetBackupConfigurationByIdsAsync(criteria.ConfigurationIds.ToList());
            }
            else
            {
                entityResultSet = await configurationRepository.GetBackupConfigurationByNamespaceAsync(criteria.Namespace);
            }

            // convert entities to dtos
            List<Ellucian.Colleague.Dtos.Base.BackupConfiguration> dtoResultSet = new List<BackupConfiguration>();
            var backupConfigDtoAdapter = _adapterRegistry.GetAdapter<
                Ellucian.Colleague.Domain.Base.Entities.BackupConfiguration,
                Ellucian.Colleague.Dtos.Base.BackupConfiguration>();
            dtoResultSet.AddRange(entityResultSet.Select(e => backupConfigDtoAdapter.MapToType(e)).ToList());
            return dtoResultSet;
        }

        /// <summary>
        /// Backs up this API instance's config data to the Colleague DB
        /// </summary>
        /// <returns></returns>
        public async Task BackupApiConfigurationAsync()
        {
            // verify permission
            if (!HasPermission(CreateApiBackupConfigPermission))
            {
                base.logger.Error("User does not have permission" + CreateApiBackupConfigPermission + " to create backup API configuration records.");
                throw new PermissionsException("User does not have permission to create backup API configuration records.");
            }

            var backupData = new Domain.Base.Entities.BackupConfiguration()
            {
                // namespace e.g. "Ellucian/Colleague Web API/1.18.0.0/dvetk_wstst01_rt"
                Namespace = ApiProductInfo.ProductNamespace + environmentName,
                ProductId = ApiProductInfo.ProductId,
                ProductVersion = ApiProductInfo.ProductVersion,
                ConfigVersion = "1",
                ConfigData = GetAPIConfigData()

            };

            await configurationRepository.AddBackupConfigurationAsync(backupData);
            base.logger.Info("API configuration backed up successfully.");
        }

        /// <summary>
        /// Restores this API instance's config data using the latest backup retrieved from Colleague DB.
        /// An optional date time filter can be used.
        /// Also optionally perform merging of any applicable setting, such as the resource files. 
        /// Note: the resx file merge operation currently is supported on a brand new API instance only. It will
        /// not run on an instance whose resource files have already been modified.
        /// </summary>
        /// <returns></returns>
        public async Task<ApiBackupConfigData> RestoreApiBackupConfigurationAsync()
        {
            // verify permission
            if (!HasPermission(RestoreApiConfigPermission))
            {
                base.logger.Error("User does not have permission" + RestoreApiConfigPermission + " to restore API configuration.");
                throw new PermissionsException("User does not have permission to restore API configuration.");
            }
            string nameSpace = ApiProductInfo.ProductNamespace + environmentName;
            var backupDataResultList = await configurationRepository.GetBackupConfigurationByNamespaceAsync(nameSpace);

            if (backupDataResultList == null || backupDataResultList.Count == 0)
            {
                throw new Exception("No backup config data record found for namespace " + nameSpace);
            }

            // use the record with the latest time stamp
            var latestRecord = backupDataResultList.OrderByDescending(bu => bu.CreatedDateTime).FirstOrDefault();
            if (latestRecord == null || string.IsNullOrEmpty(latestRecord.ConfigData))
            {
                throw new Exception("Latest backup config data record for " + nameSpace + " is empty.");
            }
            ApiBackupConfigData apiBackupConfigData = JsonConvert.DeserializeObject<ApiBackupConfigData>(latestRecord.ConfigData);

            // *** Restoring ***
            // restore api settings by simply replacing it with the backup data
            xmlSettingsRepository.Update(apiBackupConfigData.Settings);
            logger.Info("settings.config restored.");

            // API profile settings stored in WEB.API.CONFIG don't need to be restored 
            // since they are already centrally stored in Colleague DB.
            // ApiSettingsRepository.Update(apiBackupConfigData.ApiSettings);

            // however, we do want to restore the various appsettings in web.config
            AddOrUpdateAppSettings("BulkReadSize", apiBackupConfigData.ApiSettings.BulkReadSize.ToString());
            AddOrUpdateAppSettings("IncludeLinkSelfHeaders", apiBackupConfigData.ApiSettings.IncludeLinkSelfHeaders.ToString());
            AddOrUpdateAppSettings("EnableConfigBackup", apiBackupConfigData.ApiSettings.EnableConfigBackup.ToString());
            AddOrUpdateAppSettings("AttachRequestMaxSize", apiBackupConfigData.ApiSettings.AttachRequestMaxSize.ToString());
            AddOrUpdateAppSettings("DetailedHealthCheckApiEnabled", apiBackupConfigData.ApiSettings.DetailedHealthCheckApiEnabled.ToString());

            // plus any other appSettings values in web.config that's not in appsettings
            var maxQueryAttributeLimit = apiBackupConfigData.WebConfigAppSettingsMaxQueryAttributeLimit;
            if (!string.IsNullOrEmpty(maxQueryAttributeLimit))
            {
                AddOrUpdateAppSettings("MaxQueryAttributeLimit", maxQueryAttributeLimit);
            }

            logger.Info("Web.config appSettings restored.");

            // *** Merging ***
            // For API, the resource files are the only thing that may need to be merged.

            // IMPORTANT: the merge utility assumes the existing resource files are Ellucian-delivered
            // and bases its logic accordingly. Because of that, this merge will not be done on an API instance
            // whose resource files have already been customized. it will only be done on a brand new instance.
            // In SaaS, it would typically run when we spin up new instances or replacement instance 
            // after a recycle or upgrade.

            if (!string.IsNullOrWhiteSpace(apiBackupConfigData.ResourceFileChangeLogContent))
            {
                // if there's anything to merge
                var changeLogPath = resourceRepository.ChangeLogPath;
                if (string.IsNullOrWhiteSpace(changeLogPath))
                {
                    logger.Warn("Resource file change log path could not be determined. Skipping merging of resource files.");
                }
                if (File.Exists(changeLogPath))
                {
                    // first detect whether this instance has modified resource files, by checking whether the
                    // change log file already exists. If so, skip the merging step.
                    logger.Info("Resource file change log exists, which means this API instance has modified resource files. Skipping merging of resource files.");
                }
                else
                {
                    // otherwise, create a change log file with the log content from the backup data, and perform the merge
                    File.WriteAllText(changeLogPath, apiBackupConfigData.ResourceFileChangeLogContent);
                    var appPath = System.Web.HttpRuntime.AppDomainAppPath;
                    var mergeLogger = new InstallLogger(Path.Combine(appPath, @"App_Data\Logs"), "resx_merge");
                    BackupUtility buUtil = new BackupUtility(appPath, "", mergeLogger);
                    buUtil.MergeAppGlobalResources();
                    logger.Info("Resource file merging completed.");
                }
            }
            base.logger.Info("API configuration restored successfully using backup record " + latestRecord.Id);
            return apiBackupConfigData;
        }

        private string GetAPIConfigData()
        {
            // gather settings from settings.config, web.config, resource change log
            // and put all that into an ApiBackupConfigData object
            ApiBackupConfigData backupData = new ApiBackupConfigData();
            backupData.ApiSettings = apiSettings;
            backupData.Settings = xmlSettingsRepository.Get();

            var changeLogPath = resourceRepository.ChangeLogPath;
            if (string.IsNullOrWhiteSpace(changeLogPath))
            {
                logger.Warn("Resource file change log path could not be determined. Skipping backing up of resource change log.");
            }
            else
            {
                if (File.Exists(changeLogPath))
                {
                    var resxChangeLogFileContent = File.ReadAllText(changeLogPath);
                    backupData.ResourceFileChangeLogContent = resxChangeLogFileContent;
                }
                else
                {
                    logger.Info("Resource file change log does not exist.");
                }
            }

            // backup the optional MaxQueryAttributeLimit setting
            backupData.WebConfigAppSettingsMaxQueryAttributeLimit = null;
            var MaxQueryAttributeLimitSetting = ConfigurationManager.AppSettings["MaxQueryAttributeLimit"];
            if (!string.IsNullOrEmpty(MaxQueryAttributeLimitSetting))
            {
                backupData.WebConfigAppSettingsMaxQueryAttributeLimit = MaxQueryAttributeLimitSetting;
            }
            return JsonConvert.SerializeObject(backupData);
        }

        private static void AddOrUpdateAppSettings(string key, string value)
        {
            try
            {
                var configFile = WebConfigurationManager.OpenWebConfiguration("~");
                var settings = configFile.AppSettings.Settings;
                if (settings[key] == null)
                {
                    settings.Add(key, value);
                }
                else
                {
                    settings[key].Value = value;
                }
                configFile.Save(ConfigurationSaveMode.Minimal);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Error writing app settings");
            }
        }

        #endregion

        /// <summary>
        /// Retrieves <see cref="SelfServiceConfiguration"/>
        /// </summary>
        /// <returns>A <see cref="SelfServiceConfiguration"/> object</returns>
        public async Task<Dtos.Base.SelfServiceConfiguration> GetSelfServiceConfigurationAsync()
        {
            var configuration = await configurationRepository.GetSelfServiceConfigurationAsync();
            var adapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.SelfServiceConfiguration, Dtos.Base.SelfServiceConfiguration>();
            var configurationDto = adapter.MapToType(configuration);
            return configurationDto;
        }

        /// <summary>
        /// Calls configuration repository and returns the Required Document Configuration data transfer object.
        /// </summary>
        /// <returns><see cref="Dtos.Base.RequiredDocumentConfiguration">Required Document Configuration</see> data transfer object.</returns>
        public async Task<Dtos.Base.RequiredDocumentConfiguration> GetRequiredDocumentConfigurationAsync()
        {
            var configuration = await configurationRepository.GetRequiredDocumentConfigurationAsync();
            var adapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.RequiredDocumentConfiguration, Dtos.Base.RequiredDocumentConfiguration>();
            var configurationDto = adapter.MapToType(configuration);
            return configurationDto;
        }

        /// <summary>
        /// Returns the session configuration
        /// </summary>
        /// <returns>Session configuration</returns>
        public async Task<Dtos.Base.SessionConfiguration> GetSessionConfigurationAsync()
        {
            var configuration = await configurationRepository.GetSessionConfigurationAsync();
            var adapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.SessionConfiguration, Dtos.Base.SessionConfiguration>();
            var configurationDto = adapter.MapToType(configuration);
            return configurationDto;
        }


        ///////////////////////////////////////////////////////////////////////////////////
        ///                                                                             ///
        ///                               CF Team                                       ///                                                                             
        ///                         TAX INFORMATION VIEWS                               ///
        ///           TAX FORMS CONFIGURATION, CONSENTs, STATEMENTs, PDFs               ///
        ///                                                                             ///
        ///////////////////////////////////////////////////////////////////////////////////

        #region CF Views

        /// <summary>
        /// Gets the tax form configuration.
        /// </summary>
        /// <param name="taxForm>The tax form (W-2, 1095-C, 1098-T, etc.)</param>
        /// <returns>Tax form configuration DTO.</returns>
        public async Task<Dtos.Base.TaxFormConfiguration2> GetTaxFormConsentConfiguration2Async(string taxForm)
        {
            if (string.IsNullOrWhiteSpace(taxForm))
                throw new ArgumentNullException("taxForm", "The tax form type must be specified.");


            switch (taxForm)
            {
                case Domain.Base.TaxFormTypes.FormW2:
                case Domain.Base.TaxFormTypes.Form1095C:
                case Domain.Base.TaxFormTypes.Form1098:
                case Domain.Base.TaxFormTypes.FormT4:
                case Domain.Base.TaxFormTypes.FormT4A:
                case Domain.Base.TaxFormTypes.FormT2202A:
                case Domain.Base.TaxFormTypes.Form1099MI:
                case Domain.Base.TaxFormTypes.Form1099NEC:
                    break;
                default:
                    throw new ArgumentNullException("taxForm", "Invalid tax form.");
            }

            // Retrieve the configuration parameters for this tax form.
            var configuration = await this.configurationRepository.GetTaxFormConsentConfiguration2Async(taxForm);

            if (configuration == null)
                throw new ArgumentNullException("configuration", "configuration cannot be null.");

            var taxFormConfiguration2DtoAdapter = new TaxFormConfiguration2EntityToDtoAdapter(_adapterRegistry, base.logger);
            var configurationDto = taxFormConfiguration2DtoAdapter.MapToType(configuration);


            return configurationDto;
        }

        #endregion

        #region OBSOLETE METHODS

        /// <summary>
        /// Gets the tax form configuration.
        /// </summary>
        /// <param name="taxFormId">The tax form (W-2, 1095-C, 1098-T, etc.)</param>
        /// <returns>Tax form configuration DTO.</returns>
        [Obsolete("Obsolete as of API 1.29.1. Use GetTaxFormConsentConfiguration2Async instead.")]
        public async Task<Dtos.Base.TaxFormConfiguration> GetTaxFormConsentConfigurationAsync(Dtos.Base.TaxForms taxFormId)
        {
            Domain.Base.Entities.TaxForms taxFormDomain = new Domain.Base.Entities.TaxForms();

            // Convert the tax form dto into the domain dto.
            switch (taxFormId)
            {
                case Dtos.Base.TaxForms.FormW2:
                    taxFormDomain = Domain.Base.Entities.TaxForms.FormW2;
                    break;
                case Dtos.Base.TaxForms.Form1095C:
                    taxFormDomain = Domain.Base.Entities.TaxForms.Form1095C;
                    break;
                case Dtos.Base.TaxForms.Form1098:
                    taxFormDomain = Domain.Base.Entities.TaxForms.Form1098;
                    break;
                case Dtos.Base.TaxForms.FormT4:
                    taxFormDomain = Domain.Base.Entities.TaxForms.FormT4;
                    break;
                case Dtos.Base.TaxForms.FormT4A:
                    taxFormDomain = Domain.Base.Entities.TaxForms.FormT4A;
                    break;
                case Dtos.Base.TaxForms.FormT2202A:
                    taxFormDomain = Domain.Base.Entities.TaxForms.FormT2202A;
                    break;
                case Dtos.Base.TaxForms.Form1099MI:
                    taxFormDomain = Domain.Base.Entities.TaxForms.Form1099MI;
                    break;
            }

            // Retrieve the configuration parameters for this tax form
            var configuration = await this.configurationRepository.GetTaxFormConsentConfigurationAsync(taxFormDomain);

            if (configuration == null)
                throw new ArgumentNullException("configuration", "configuration cannot be null.");

            var taxFormConfigurationDtoAdapter = new TaxFormConfigurationEntityToDtoAdapter(_adapterRegistry, base.logger);
            var configurationDto = taxFormConfigurationDtoAdapter.MapToType(configuration);

            return configurationDto;
        }

        #endregion
    }
}