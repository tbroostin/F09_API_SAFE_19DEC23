// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.App.Config.Storage.Service.Client;
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
                return new Domain.Base.Entities.BackupConfiguration();
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
                logger.Warn("Exception occurred reading API profile \"" + backupData.Settings.ProfileName + "\".", e);
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
                        logger.Error("Could not write bytes to file " + entry.Key, e);
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
                                logger.Error("Could not write to file " + entry.Key, e);
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
                                logger.Error("Could not write to file " + entry.Key, e);
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
                logger.Error("Error writing app settings", e);
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
                logger.Error("Error retrieving binary files", e);
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
                logger.Error("Error retrieving resource files", e);
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