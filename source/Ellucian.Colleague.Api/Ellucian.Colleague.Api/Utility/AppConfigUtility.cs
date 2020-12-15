// Copyright 2019-2020 Ellucian Company L.P. and its affiliates.
using Ellucian.App.Config.Storage.Service.Client;
using Ellucian.Colleague.Api.Models;
using Ellucian.Web.Http.Configuration;
using Ellucian.Web.Mvc.Install;
using Ellucian.Web.Mvc.Install.Backup;
using Ellucian.Web.Resource;
using Newtonsoft.Json;
using slf4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.Mvc;

namespace Ellucian.Colleague.Api.Utility
{
    /// <summary>
    /// Utility for managing configuration for this application
    /// </summary>
    public class AppConfigUtility
    {
        /// <summary>
        /// API config version
        /// MUST be incremented everytime any setting/property is added/removed/renamed in any of the setting groups.
        /// </summary>
        public const string ApiConfigVersion = "1.0";

        /// <summary>
        /// config service client settings
        /// </summary>
        public static ConfigStorageServiceClientSettings ConfigServiceClientSettings = Utilities.GetConfigMonitorSettings();

        /// <summary>
        /// Config service client for sending requests to the storage service.
        /// This static client gets set by the first call to GetApiConfigurationObject() below, 
        /// which provides this instance's namespace that the client requires.
        /// </summary>
        public static ConfigStorageServiceHttpClient StorageServiceClient;

        /// <summary>
        /// Get back the overall config object which contains all of API's various config data objects
        /// </summary>
        /// <returns></returns>
        public static Domain.Base.Entities.BackupConfiguration GetApiConfigurationObject()
        {
            ISettingsRepository settingsRepo = DependencyResolver.Current.GetService<ISettingsRepository>();
            ApiBackupConfigData backupData = new ApiBackupConfigData();
            backupData.Settings = settingsRepo.Get();
            var nameSpace = "Ellucian/" + ApiProductInfo.ProductId + '/' + backupData.Settings.ColleagueSettings.DmiSettings.AccountName;

            // Initialize the storage service client as soon as we have the namespace
            if (StorageServiceClient == null)
            {
                StorageServiceClient = new ConfigStorageServiceHttpClient(ConfigServiceClientSettings, ApiConfigVersion, nameSpace);
            }

            if (string.IsNullOrEmpty(backupData.Settings.ColleagueSettings.DmiSettings.IpAddress) 
                || backupData.Settings.ColleagueSettings.DmiSettings.Port == 0)
            {
                // this is a brand new instance with no setting configured. 
                // Just return an empty config object. The monitor job will start and if there's any backup config data, it will be restored.
                // Note: this should not happen for SaaS as the basic connection parms are set as part of provisioning.
                var blankConfig = new Domain.Base.Entities.BackupConfiguration()
                {
                    Namespace = nameSpace,
                    ProductId = ApiProductInfo.ProductId,
                    ProductVersion = ApiProductInfo.ProductVersion,
                    ConfigVersion = ApiConfigVersion,
                };
                return blankConfig;
            }

            IApiSettingsRepository apiSettingsRepo = DependencyResolver.Current.GetService<IApiSettingsRepository>();
            IResourceRepository resourceRepo = DependencyResolver.Current.GetService<IResourceRepository>();
            ILogger logger = DependencyResolver.Current.GetService<ILogger>();

            ApiSettings apiSettings = null;
            try
            {
                // always get the fresh apiSettings from the repo instead of the cached object in dependencyresolver, 
                // as its version number automatically increases every save, and may not match what's in memory.
                apiSettings = apiSettingsRepo.Get(backupData.Settings.ProfileName);
            }catch(Exception e)
            {
                logger.Warn(e, "Exception occurred reading API profile \"" + backupData.Settings.ProfileName + "\".");
            }

            backupData.ApiSettings = apiSettings;
            backupData.BinaryFiles = GetBinaryFiles(logger, apiSettings);

            var changeLogPath = resourceRepo.ChangeLogPath;
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
                    backupData.ResourceFileChangeLogContent = null;
                }
            }

            backupData.ResourceFiles = GetResourceFiles(logger, resourceRepo.BaseResourcePath, changeLogPath);

            // backup the optional MaxQueryAttributeLimit setting
            backupData.WebConfigAppSettingsMaxQueryAttributeLimit = null;
            var MaxQueryAttributeLimitSetting = ConfigurationManager.AppSettings["MaxQueryAttributeLimit"];
            if (!string.IsNullOrEmpty(MaxQueryAttributeLimitSetting))
            {
                backupData.WebConfigAppSettingsMaxQueryAttributeLimit = MaxQueryAttributeLimitSetting;
            }

            // encrypt any secrets in this backup object before serializing it.
            backupData = EncryptSecrets(backupData);

            string configJson = JsonConvert.SerializeObject(backupData);

            var configData = new Domain.Base.Entities.BackupConfiguration()
            {
                // namespace e.g. "Ellucian/Colleague Web API/dvetk_wstst01_rt"
                // This is used as the "ID" for the set of config records that this instance consumes.

                // ***IMPORTANT*** do not include any kind of product version in the namespace. If you do,
                // later version cannot get config data provided by a previous version, and this would break
                // the very popular "upgrade" scenario, where an instance with a newer version spins up to replace the older one.

                Namespace = nameSpace,
                ProductId = ApiProductInfo.ProductId,
                ProductVersion = ApiProductInfo.ProductVersion,
                ConfigVersion = ApiConfigVersion,
                ConfigData = configJson
            };
            logger.Info("Successfully built the API configuration object for backup.");
            return configData;
        }

        /// <summary>
        /// Restores this API instance's config data using the latest backup retrieved from Colleague DB.
        /// An optional date time filter can be used.
        /// Also optionally perform merging of any applicable settings, such as the resource files. 
        /// </summary>
        /// <returns></returns>
        public static void RestoreApiBackupConfiguration(string configData)
        {
            IApiSettingsRepository apiSettingsRepo = DependencyResolver.Current.GetService<IApiSettingsRepository>();
            ISettingsRepository settingsRepo = DependencyResolver.Current.GetService<ISettingsRepository>();
            IResourceRepository resourceRepo = DependencyResolver.Current.GetService<IResourceRepository>();
            ILogger logger = DependencyResolver.Current.GetService<ILogger>();
            ApiBackupConfigData apiBackupConfigData = JsonConvert.DeserializeObject<ApiBackupConfigData>(configData);

            // decrypt any secrets in this backup object before restoring it.
            apiBackupConfigData = DecryptSecrets(apiBackupConfigData);

            // *** Restoring ***
            // restore api settings by simply replacing it with the backup data
            settingsRepo.Update(apiBackupConfigData.Settings); // no merging logic available
            logger.Info("settings.config restored.");

            // API profile settings stored in WEB.API.CONFIG don't need to be restored 
            // since they are already centrally stored in Colleague DB.
            // ApiSettingsRepository.Update(apiBackupConfigData.ApiSettings);
            // However, we will always replace the binary files which don't get stored in Colleague:
            if (apiBackupConfigData.BinaryFiles != null && apiBackupConfigData.BinaryFiles.Any())
            {
                foreach (KeyValuePair<string, string> entry in apiBackupConfigData.BinaryFiles)
                {
                    var fileMapPath = HostingEnvironment.MapPath(entry.Key); // these paths are relative paths
                    var fileBytes = Convert.FromBase64String(entry.Value);
                    try
                    {
                        File.WriteAllBytes(fileMapPath, fileBytes); // this will overwrite existing file of same name.
                        logger.Info("Wrote bytes " + entry.Key);
                    }
                    catch (Exception e)
                    {
                        logger.Error(e, "Could not write bytes to file " + entry.Key);
                    }
                }
            }

            // however, we do want to restore the various appsettings in web.config
            if (apiBackupConfigData.ApiSettings != null)
            {
                try
                {
                    AddOrUpdateAppSettings(logger, "BulkReadSize", apiBackupConfigData.ApiSettings.BulkReadSize.ToString());
                    AddOrUpdateAppSettings(logger, "IncludeLinkSelfHeaders", apiBackupConfigData.ApiSettings.IncludeLinkSelfHeaders.ToString());
                    AddOrUpdateAppSettings(logger, "EnableConfigBackup", apiBackupConfigData.ApiSettings.EnableConfigBackup.ToString());
                    AddOrUpdateAppSettings(logger, "AttachRequestMaxSize", apiBackupConfigData.ApiSettings.AttachRequestMaxSize.ToString());
                    AddOrUpdateAppSettings(logger, "DetailedHealthCheckApiEnabled", apiBackupConfigData.ApiSettings.DetailedHealthCheckApiEnabled.ToString());
                }
                catch (Exception e)
                {
                    logger.Warn(e, "Exception occurred restoring web.config appsettings.");
                }
            }

            // plus any other appSettings values in web.config that's not in appsettings
            var maxQueryAttributeLimit = apiBackupConfigData.WebConfigAppSettingsMaxQueryAttributeLimit;
            if (!string.IsNullOrEmpty(maxQueryAttributeLimit))
            {
                AddOrUpdateAppSettings(logger, "MaxQueryAttributeLimit", maxQueryAttributeLimit);
            }

            logger.Info("Web.config appSettings restored.");

            // *** Resource File Restoration ***
            /*
             *
             * Scenario 1: This is a load balancing instance that needs to sync with one where a config change was made.
             * Method 1 is used: All resource files and change log file will be replaced with the ones from the backup config data.
             * 
             * Scenario 2: This is a load balancing instance that needs to sync with one where the resource file customizations have been
             * undone and the resource changelog file deleted.
             * Method 2 is used: copy over the resource files and delete the changelog file.
             * 
             * 
             * Scenario 3: This is a new instance for scaling or for replacing a failed instance, or is an upgrade replacement.
             * Method 3 is used: The merge logic will apply changes from the change log to the uncustomized resource files on this new instance.
             * If the changelog file is not present in the backup config data, no merge will occur.
             * This merge will cover the upgrade scenario where delivered resource files have different entries.
             *  
             */
            var changeLogPath = resourceRepo.ChangeLogPath;
            if (!string.IsNullOrWhiteSpace(changeLogPath) && File.Exists(changeLogPath))
            {
                // This instance's change log file already exists. If so, this is not a brand new instance, so we 
                // need to update all resource files and change log by overwriting them.

                if (apiBackupConfigData.ResourceFileChangeLogContent != null)
                {
                    // Scenario 1: This is a load balancing instance that needs to sync with one where a config change was made.
                    // Method 1: All resource files and change log file will be replaced with the ones from the backup config data.
                    logger.Info("Resource file change log exists, which means this API instance has modified resource files. Overwrite instead of merge resource files.");
                    if (apiBackupConfigData.ResourceFiles != null && apiBackupConfigData.ResourceFiles.Any())
                    {
                        foreach (KeyValuePair<string, string> entry in apiBackupConfigData.ResourceFiles)
                        {
                            try
                            {
                                var physicalPath = HostingEnvironment.MapPath(entry.Key);
                                File.WriteAllText(physicalPath, entry.Value);
                                logger.Info("Wrote file " + physicalPath);
                            }
                            catch(Exception e)
                            {
                                logger.Error(e, "Could not write to file " + entry.Key);
                            }
                        }
                        logger.Info("Resource files replaced.");
                    }else
                    {
                        logger.Info("No action was taken for resource files since backup data contain no resource files.");
                    }
                }
                else
                {
                    // Scenario 2: This is a load balancing instance that needs to sync with one where all resource file customizations have been
                    // undone and the resource changelog file deleted.
                    // Method 2: overwrite the resource files with ones from backup data, and delete the instance's existing change log file.
                    if (apiBackupConfigData.ResourceFiles != null && apiBackupConfigData.ResourceFiles.Any())
                    {
                        foreach (KeyValuePair<string, string> entry in apiBackupConfigData.ResourceFiles)
                        {
                            try
                            {
                                var physicalPath = HostingEnvironment.MapPath(entry.Key);
                                File.WriteAllText(physicalPath, entry.Value);
                                logger.Info("Wrote file " + physicalPath);
                            }
                            catch (Exception e)
                            {
                                logger.Error(e, "Could not write to file " + entry.Key);
                            }
                        }
                        File.Delete(changeLogPath);
                        logger.Info("Resource files replaced, and changelog file deleted.");
                    }
                    else
                    {
                        logger.Info("No action was taken for resource files since backup data contain no resource files.");
                    }
                }
            }
            else
            {
                // This instance's change log file does not exist. 
                // Scenario 3: This is a new instance for scaling or for replacing a failed instance, or is an upgrade replacement.
                // resource file customization.
                if (apiBackupConfigData.ResourceFileChangeLogContent != null)
                {
                    // There's resource changes from the backup config data to be applied.

                    // Method 3:
                    // Create a change log file with the log content from the backup data, and perform the merge using the existing
                    // resouce file merge utility.
                    // NOTE: the merge utility assumes the existing resource files are Ellucian-delivered / uncustomized
                    // and bases its logic accordingly. This is why this scenario is the 

                    File.WriteAllText(changeLogPath, apiBackupConfigData.ResourceFileChangeLogContent);
                    var appPath = System.Web.HttpRuntime.AppDomainAppPath;
                    var mergeLogger = new InstallLogger(Path.Combine(appPath, @"App_Data\Logs"), "resx_merge");
                    BackupUtility buUtil = new BackupUtility(appPath, "", mergeLogger);
                    buUtil.MergeAppGlobalResources();
                    logger.Info("Resource file merging completed.");
                }else
                {
                    // do nothing, as there are no resource changes in either the instance or the backup config data
                    logger.Info("No action was taken for resource files, since there are no resource file changes in either the instance or the backup config data.");
                }
            }
            logger.Info("API configuration restored/updated successfully.");
        }

        /// <summary>
        /// Apply config changes from a valid staging config file.
        /// This action will occur only once. The staging config file will be archived after it is processed and will not be processed again.
        /// </summary>
        /// <returns>True if changes occurred. False if no changes made.</returns>
        public static bool ApplyStagingConfigFile()
        {
            var logger = DependencyResolver.Current.GetService<ILogger>();
            var stagingConfig = GetStagingConfig(logger);
            bool changesOccurred = false;
            if (stagingConfig != null)
            {
                logger.Info("Valid staging config file found, applying it...");
                try
                {
                    changesOccurred = AppConfigUtility.UpdateConfigsWithStagingData(logger, stagingConfig);
                    logger.Info("Staging config file has been processed.");
                }
                catch(Exception e)
                {
                    logger.Error(e, "Error occurred processing staging config data. ");
                };

                // archive the staging config file once it's processed, so it doesn't get reapplied evert startup.
                // this is done even in the event that the processing failed, so the app doesn't get stuck in a loop.
                ArchiveStagingConfigFile(logger);
                logger.Info("Staging config file has been archived and will not be processed again on next startup.");
            }
            return changesOccurred;
        }

        /// <summary>
        /// Similar to the restore method, but the source is the staging config file.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="stagingConfig"></param>
        /// <returns>True if changes occurred. False if no changes made.</returns>
        private static bool UpdateConfigsWithStagingData(ILogger logger, SaasStagingConfiguration stagingConfig)
        {
            // Go through each supported config for staging, and update it in the supplied config data if it is present in the staging file.
            // Document supported staging configs here: 
            // https://confluence.ellucian.com/display/colleague/Config+Staging+File+Template#ConfigStagingFileTemplate-Supportedstagingconfigsbyproduct

            bool changesOccurred = false;

            // ------- all general settings ------------

            ISettingsRepository settingsRepo = DependencyResolver.Current.GetService<ISettingsRepository>();
            Settings currentSettings = null;

            // Set log level
            string logLevelSettingName = "log level";
            var newLogLevelSetting = GetSettingFromStagingConfigFile(stagingConfig, logLevelSettingName);
            if (newLogLevelSetting != null && !string.IsNullOrWhiteSpace(newLogLevelSetting.SettingValue))
            {
                if (currentSettings == null)
                {
                    currentSettings = settingsRepo.Get();
                }
                var newLogLevelString = newLogLevelSetting.SettingValue;
                var oldLogLevel = currentSettings.LogLevel.ToString();

                System.Diagnostics.SourceLevels newLogLevel;
                if (Enum.TryParse(newLogLevelString, true, out newLogLevel))
                {
                    var newSettings = new Settings(currentSettings.ColleagueSettings, newLogLevel) { ProfileName = currentSettings.ProfileName };
                    currentSettings = newSettings; 
                    changesOccurred = true;
                    logger.Info(string.Format("Staging file changes: {0}. Old value={1}; new value={2}", logLevelSettingName, oldLogLevel, newLogLevelString));
                }
                else
                {
                    logger.Error(string.Format("Staging file changes: Invalid input for setting {0}: {1}", logLevelSettingName, newLogLevelString));
                }
            }

            // Set api settings profile name
            string apiProfileNameSettingName = "api profile name";
            var newApiProfileNameSetting = GetSettingFromStagingConfigFile(stagingConfig, apiProfileNameSettingName);
            if (newApiProfileNameSetting != null && !string.IsNullOrWhiteSpace(newApiProfileNameSetting.SettingValue))
            {
                if (currentSettings == null)
                {
                    currentSettings = settingsRepo.Get();
                }
                var newApiProfileNameString = newApiProfileNameSetting.SettingValue;
                var oldApiProfileName = currentSettings.ProfileName;

                if (!string.IsNullOrWhiteSpace(newApiProfileNameString))
                {
                    var newSettings = new Settings(currentSettings.ColleagueSettings, currentSettings.LogLevel) { ProfileName = newApiProfileNameString };
                    currentSettings = newSettings;
                    changesOccurred = true;
                    logger.Info(string.Format("Staging file changes: {0}. Old value={1}; new value={2}", apiProfileNameSettingName, oldApiProfileName, newApiProfileNameString));
                }
                else
                {
                    logger.Error(string.Format("Staging file changes: Invalid input for setting {0}: {1}", apiProfileNameSettingName, newApiProfileNameString));
                }
            }

            // set shared secret
            string apiSharedSecretSettingName = "api shared secret";
            var newApiSharedSecretSetting = GetSettingFromStagingConfigFile(stagingConfig, apiSharedSecretSettingName);
            if (newApiSharedSecretSetting != null && !string.IsNullOrWhiteSpace(newApiSharedSecretSetting.SettingValue))
            {
                if (currentSettings == null)
                {
                    currentSettings = settingsRepo.Get();
                }
                var newApiSharedSecretString = newApiSharedSecretSetting.SettingValue;
                string decryptedNewApiSharedSecret = null;
                try
                {
                    decryptedNewApiSharedSecret = Utilities.DecryptString(newApiSharedSecretString);
                }catch(Exception e)
                {
                    logger.Error(e, string.Format("Staging file changes: Decryption failed for {0}, error: {1}", apiSharedSecretSettingName, e.Message));
                }

                if (!string.IsNullOrWhiteSpace(decryptedNewApiSharedSecret))
                {
                    var oldApiSharedSecret = currentSettings.ColleagueSettings.DmiSettings.SharedSecret;

                    var newColleagueSettings = currentSettings.ColleagueSettings;
                    newColleagueSettings.DmiSettings.SharedSecret = decryptedNewApiSharedSecret;
                    var newSettings = new Settings(currentSettings.ColleagueSettings, currentSettings.LogLevel) { ProfileName = currentSettings.ProfileName };
                    currentSettings = newSettings;
                    changesOccurred = true;
                    logger.Info(string.Format("Staging file changes: {0}. Old value={1}; new value={2}", apiSharedSecretSettingName, "*notshown*", "*notshown*"));
                }
                else
                {
                    logger.Error(string.Format("Staging file changes: Invalid input for setting {0}: {1}", apiSharedSecretSettingName, "*not shown*"));
                }
            }

            if (currentSettings != null && changesOccurred)
            {
                // object currentSettings now has all the new settings applied. Update it.
                settingsRepo.Update(currentSettings);
                logger.Info("Staging file changes: updated 'settings' object.");

                //logger.Info("new settings=" + JsonConvert.SerializeObject(currentSettings)); for debugging only, contains plaintext secrets.
            }            

            // ------ all WEB.API.CONFIG related settings -----------

            IApiSettingsRepository apiSettingsRepo = DependencyResolver.Current.GetService<IApiSettingsRepository>();
            ApiSettings currentApiSettings = null;

            // Note: the binary file paths are part of the profile setting, which is stored in data base table UT.PARMS WEB.API.CONFIG

            // set report logo file (encoded binary string)
            var newReportLogoFileSetting = GetSettingFromStagingConfigFile(stagingConfig, "report logo file");
            if (newReportLogoFileSetting != null && !string.IsNullOrWhiteSpace(newReportLogoFileSetting.SettingValue))
            {
                var newReportLogoFileString = newReportLogoFileSetting.SettingValue;
                if (string.IsNullOrWhiteSpace(newReportLogoFileString))
                {
                    logger.Error("Staging file changes: Invalid report logo file setting - value is empty.");
                }
                else
                {
                    // get report logo path
                    if (currentSettings == null)
                    {
                        currentSettings = settingsRepo.Get();
                    }
                    if (currentApiSettings == null)
                    {
                        try
                        {
                            currentApiSettings = apiSettingsRepo.Get(currentSettings.ProfileName);
                        }
                        catch (Exception e)
                        {
                            logger.Error(e, "Staging file changes: could not read API profile setting record.");
                        }
                    }
                    string reportLogoPath = null;
                    if (currentApiSettings != null)
                    {
                        reportLogoPath = currentApiSettings.ReportLogoPath;
                        if (!string.IsNullOrWhiteSpace(reportLogoPath) && !reportLogoPath.StartsWith("~"))
                        {
                            reportLogoPath = "~" + reportLogoPath;
                        }
                    }
                    if (string.IsNullOrWhiteSpace(reportLogoPath))
                    {
                        reportLogoPath = "~/Images/report-logo.png";
                        logger.Info("Staging file changes: no current report logo file path is set, using SAAS default - " + reportLogoPath);
                    }

                    byte[] fileBytes = null;
                    try
                    {
                        fileBytes = Convert.FromBase64String(newReportLogoFileString);
                    }
                    catch (Exception e)
                    {
                        logger.Error(e, "Staging file changes: Could not base64-decode report logo file string.");
                    }

                    if (fileBytes != null)
                    {
                        try
                        {
                            var fileMapPath = HostingEnvironment.MapPath(reportLogoPath); // these paths are relative paths
                            File.WriteAllBytes(fileMapPath, fileBytes); // this will overwrite existing file of same name.
                            changesOccurred = true;
                            logger.Info("Staging file changes: Updated report logo file at " + reportLogoPath);
                        }
                        catch (Exception e)
                        {
                            logger.Error(e, "Staging file changes: Could not write bytes to report logo file path " + reportLogoPath);
                        }
                    }
                }
            }

            // set unofficial watermark file (encoded binary string)
            var newUnofficialWatermarkFileSetting = GetSettingFromStagingConfigFile(stagingConfig, "unofficial watermark file");
            if (newUnofficialWatermarkFileSetting != null && !string.IsNullOrWhiteSpace(newUnofficialWatermarkFileSetting.SettingValue))
            {
                var newUnofficialWatermarkFileString = newUnofficialWatermarkFileSetting.SettingValue;
                if (string.IsNullOrWhiteSpace(newUnofficialWatermarkFileString))
                {
                    logger.Error("Staging file changes: Invalid unofficial watermark file setting - value is empty.");
                }
                else
                {
                    // get unofficial watermark path
                    if (currentSettings == null)
                    {
                        currentSettings = settingsRepo.Get();
                    }
                    if (currentApiSettings == null)
                    {
                        try
                        {
                            currentApiSettings = apiSettingsRepo.Get(currentSettings.ProfileName);
                        }
                        catch (Exception e)
                        {
                            logger.Error(e, "Staging file changes: could not read API profile setting record.");
                        }
                    }
                    string unofficialWatermarkPath = null;
                    if (currentApiSettings != null)
                    {
                        unofficialWatermarkPath = currentApiSettings.UnofficialWatermarkPath;
                        if (!string.IsNullOrWhiteSpace(unofficialWatermarkPath) && !unofficialWatermarkPath.StartsWith("~"))
                        {
                            unofficialWatermarkPath = "~" + unofficialWatermarkPath;
                        }
                    }
                    if (string.IsNullOrWhiteSpace(unofficialWatermarkPath))
                    {
                        unofficialWatermarkPath = "~/Images/unofficial-watermark.png";
                        logger.Info("Staging file changes: no current watermark file path is set, using SAAS default - " + unofficialWatermarkPath);
                    }

                    byte[] fileBytes = null;
                    try
                    {
                        fileBytes = Convert.FromBase64String(newUnofficialWatermarkFileString);
                    }
                    catch (Exception e)
                    {
                        logger.Error(e, "Staging file changes: Could not base64-decode unofficial watermark file string.");
                    }

                    if (fileBytes != null)
                    {
                        try
                        {
                            var fileMapPath = HostingEnvironment.MapPath(unofficialWatermarkPath); // these paths are relative paths
                            File.WriteAllBytes(fileMapPath, fileBytes); // this will overwrite existing file of same name.
                            changesOccurred = true;
                            logger.Info("Staging file changes: Updated unofficial watermark file at " + unofficialWatermarkPath);
                        }
                        catch (Exception e)
                        {
                            logger.Error(e, "Staging file changes: Could not write bytes to unofficial watermark file path " + unofficialWatermarkPath);
                        }
                    }
                }
            }

            return changesOccurred;
        }

        /// <summary>
        /// Return the UpdateSetting object from the staging config object with matching setting name
        /// </summary>
        /// <param name="stagingConfig"></param>
        /// <param name="primarySettingName"></param>
        /// <returns></returns>
        public static UpdateSetting GetSettingFromStagingConfigFile(SaasStagingConfiguration stagingConfig, string primarySettingName)
        {
            UpdateSetting setting = null;
            if (string.IsNullOrWhiteSpace(primarySettingName))
            {
                return null;
            }
            setting = stagingConfig.UpdateSettings.Where(
                        s => s.SettingName.Equals(primarySettingName, StringComparison.OrdinalIgnoreCase))
                    .FirstOrDefault();
            return setting;
        }


        private static string SaasStagingConfigFilePath = "~/App_Data/SaasStagingConfig.json";

        /// <summary>
        /// Return the staging config object, if a valid file exists.
        /// </summary>
        private static SaasStagingConfiguration GetStagingConfig(ILogger logger)
        {
            SaasStagingConfiguration stagingConfig = null;
            var path = HostingEnvironment.MapPath(SaasStagingConfigFilePath);
            if (!File.Exists(path))
            {
                logger.Info("No staging config file found at " + SaasStagingConfigFilePath);
                return null;
            }
            var json = File.ReadAllText(path);
            try
            {
                stagingConfig = JsonConvert.DeserializeObject<SaasStagingConfiguration>(json);
            }
            catch (Exception e)
            {
                logger.Error(e, "Error occurred serializing the staging config object.");
                return null;
            }

            if (!stagingConfig.ApplicationName.Equals("Web API", StringComparison.Ordinal))
            {
                logger.Error("Staging config: app mismatch - expected 'Web API', but found: " + stagingConfig.ApplicationName);
                return null;
            }

            if (!Utilities.VerifyNewConfigVersionOK(ApiConfigVersion, stagingConfig.MinimumConfigVersion))
            {
                // minimumConfigVersion <= ApiConfigVersion
                logger.Error("Staging config: minimum version " + stagingConfig.MinimumConfigVersion + " is not supported. App config version is : " + ApiConfigVersion);
                return null;
            }

            return stagingConfig;
        }

        /// <summary>
        /// Rename/archive the stagingconfigfile so that it doesn't get re-applied on next startup.
        /// </summary>
        private static void ArchiveStagingConfigFile(ILogger logger)
        {
            var path = HostingEnvironment.MapPath(SaasStagingConfigFilePath);
            if (!File.Exists(path))
            {
                return;
            }
            try
            {
                System.IO.File.Move(path, path + "_archived_" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss"));
            }
            catch (Exception e)
            {
                logger.Error(e, "Error occurred archiving staging config file.");
            }
        }

        private static ApiBackupConfigData EncryptSecrets(ApiBackupConfigData unencryptedConfigData)
        {
            var colleagueSettings = unencryptedConfigData.Settings.ColleagueSettings;

            // sharedsecret
            var dmiSettings = colleagueSettings.DmiSettings;
            var sharedSecret = dmiSettings.SharedSecret;
            var encryptedSharedSecret = Utilities.EncryptStringNoSalt(sharedSecret);
            unencryptedConfigData.Settings.ColleagueSettings.DmiSettings.SharedSecret = encryptedSharedSecret;

            // das password
            var dbPw = colleagueSettings.DasSettings.DbPassword;
            var encryptedDbPw = Utilities.EncryptStringNoSalt(dbPw);
            unencryptedConfigData.Settings.ColleagueSettings.DasSettings.DbPassword = encryptedDbPw;

            return unencryptedConfigData;
        }

        private static ApiBackupConfigData DecryptSecrets(ApiBackupConfigData encryptedConfigData)
        {
            var colleagueSettings = encryptedConfigData.Settings.ColleagueSettings;

            // sharedsecret
            var dmiSettings = colleagueSettings.DmiSettings;
            var sharedSecret = dmiSettings.SharedSecret;
            var decryptedSharedSecret = Utilities.DecryptStringNoSalt(sharedSecret);
            encryptedConfigData.Settings.ColleagueSettings.DmiSettings.SharedSecret = decryptedSharedSecret;

            // das password
            var dbPw = colleagueSettings.DasSettings.DbPassword;
            var decryptedDbPw = Utilities.DecryptStringNoSalt(dbPw);
            encryptedConfigData.Settings.ColleagueSettings.DasSettings.DbPassword = decryptedDbPw;

            return encryptedConfigData;
        }

        private static void AddOrUpdateAppSettings(ILogger logger, string key, string value)
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
            catch (ConfigurationErrorsException e)
            {
                logger.Error(e, "Error writing app settings");
            }
        }

        private static Dictionary<string, string> GetBinaryFiles(ILogger logger, ApiSettings apiSettings)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            try
            {
                string reportLogoFilePath = null;
                string unofficialWatermarkFilePath = null;

                if (apiSettings != null)
                {
                    reportLogoFilePath = apiSettings.ReportLogoPath;
                    if (!reportLogoFilePath.StartsWith("~"))
                    {
                        reportLogoFilePath = "~" + reportLogoFilePath;
                    }

                    unofficialWatermarkFilePath = apiSettings.UnofficialWatermarkPath;
                    if (!unofficialWatermarkFilePath.StartsWith("~"))
                    {
                        unofficialWatermarkFilePath = "~" + unofficialWatermarkFilePath;
                    }
                }else
                {
                    // No APISettings (likely because the specified profile record doesn't exists),
                    // that means the binary file paths are the hard-coded defaults SAAS uses, so use them instead.
                    reportLogoFilePath = "~/Images/report-logo.png";
                    unofficialWatermarkFilePath = "~/Images/unofficial-watermark.png";
                }

                if (!string.IsNullOrWhiteSpace(reportLogoFilePath))
                {
                    string reportLogoFilePhysicalPath = HostingEnvironment.MapPath(reportLogoFilePath);
                    if (File.Exists(reportLogoFilePhysicalPath))
                    {
                        var reportLogoBytes = File.ReadAllBytes(reportLogoFilePhysicalPath);
                        var reportLogoBase64String = Convert.ToBase64String(reportLogoBytes);
                        dict[reportLogoFilePath] = reportLogoBase64String;
                    }
                }

                if (!string.IsNullOrWhiteSpace(unofficialWatermarkFilePath))
                {
                    string UnofficialWatermarkFilePhysicalPath = HostingEnvironment.MapPath(unofficialWatermarkFilePath);
                    if (File.Exists(UnofficialWatermarkFilePhysicalPath))
                    {
                        var UnofficialWatermarkBytes = File.ReadAllBytes(UnofficialWatermarkFilePhysicalPath);
                        var UnofficialWatermarkBase64String = Convert.ToBase64String(UnofficialWatermarkBytes);
                        dict[unofficialWatermarkFilePath] = UnofficialWatermarkBase64String;
                    }
                }
            }
            catch(Exception e)
            {
                logger.Error(e, "Error retrieving binary files");
            }
            // dict contains relative paths only.
            return dict;
        }

        private static Dictionary<string, string> GetResourceFiles(ILogger logger, string baseResourcePath, string changelogPath)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            try
            {
                List<string> allFilePaths = Directory.GetFiles(baseResourcePath, "*.resx", SearchOption.AllDirectories).ToList();
                if (File.Exists(changelogPath))
                {
                    // also grab the changelog file as well.
                    allFilePaths.Add(changelogPath);
                }
                foreach (string filePath in allFilePaths)
                {
                    if (File.Exists(filePath))
                    {
                        string content = File.ReadAllText(filePath);
                        // Save the relative path only
                        var relativePath = GetRelativePath(filePath);
                        dict[relativePath] = content;
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(e, "Error retrieving resource files");
            }
            return dict;
        }

        private static string GetRelativePath(string physicalPath)
        {
            var rootPhysicalPath = "";
            var relativePath = "";
            if (physicalPath.Contains("App_Data"))
            {
                rootPhysicalPath = HostingEnvironment.MapPath("~/App_Data");
                relativePath = "~/App_Data" + physicalPath.Replace(rootPhysicalPath, "").Replace(@"\", "/");
            }
            else if (physicalPath.Contains("App_GlobalResources"))
            {
                rootPhysicalPath = HostingEnvironment.MapPath("~/App_GlobalResources");
                relativePath = "~/App_GlobalResources" + physicalPath.Replace(rootPhysicalPath, "").Replace(@"\", "/");
            }
            return relativePath;
        }
    }
}