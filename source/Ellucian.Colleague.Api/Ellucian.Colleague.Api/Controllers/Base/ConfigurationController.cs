// Copyright 2014-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using System.Linq;
using System.Collections.Generic;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Integration Configuration data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class ConfigurationController : BaseCompressedApiController
    {
        private readonly IAdapterRegistry adapterRegistry;
        private readonly IConfigurationService configurationService;
        private readonly IProxyService proxyService;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the ConfigurationController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="configurationService">Service of type <see cref="IConfigurationService">IConfigurationService</see></param>
        /// <param name="proxyService">Service of type <see cref="IProxyService">IProxyService</see></param>
        /// <param name="logger">Interface to Logger</param>
        public ConfigurationController(IAdapterRegistry adapterRegistry, IConfigurationService configurationService, IProxyService proxyService, ILogger logger)
        {
            this.adapterRegistry = adapterRegistry;
            this.configurationService = configurationService;
            this.proxyService = proxyService;
            this.logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN CDM</remarks>
        /// <summary>
        /// Retrieves an integration configuration.
        /// </summary>
        /// <returns>An <see cref="IntegrationConfiguration">IntegrationConfiguration</see> information</returns>
        public async Task<Ellucian.Colleague.Dtos.Base.IntegrationConfiguration> Get(string configId)
        {
            try
            {
                return await configurationService.GetIntegrationConfiguration(configId);
            }
            catch (PermissionsException peex)
            {
                logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// This method gets Tax Form Configuration for the tax form passed in.
        /// </summary>
        /// <param name="taxFormId">The tax form (W-2, 1095-C, 1098-T, etc.)</param>
        /// <returns>Tax Form Configuration for the type of tax form.</returns>
        public async Task<TaxFormConfiguration> GetTaxFormConfigurationAsync(TaxForms taxFormId)
        {
            var taxFormConfiguration = await this.configurationService.GetTaxFormConsentConfigurationAsync(taxFormId);

            return taxFormConfiguration;
        }

        /// <summary>
        /// Gets the proxy configuration
        /// </summary>
        /// <returns>Proxy configuration information.</returns>
        public async Task<ProxyConfiguration> GetProxyConfigurationAsync()
        {
            try
            {
                return await proxyService.GetProxyConfigurationAsync();
            }
            catch (Exception e)
            {
                logger.Info(e.ToString());
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Gets the User Profile Configuration
        /// </summary>
        /// <returns><see cref="UserProfileConfiguration">User Profile Configuration</see></returns>
        [Obsolete("Obsolete as of API 1.16. Use version 2 of this API instead.")]
        public async Task<UserProfileConfiguration> GetUserProfileConfigurationAsync()
        {
            try
            {
                return await configurationService.GetUserProfileConfigurationAsync();
            }
            catch (Exception ex)
            {
                logger.Error("Error occurred while retrieving User Profile Configuration: " + ex.Message);
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Gets the User Profile Configuration
        /// </summary>
        /// <returns><see cref="UserProfileConfiguration2">User Profile Configuration</see></returns>
        public async Task<UserProfileConfiguration2> GetUserProfileConfiguration2Async()
        {
            try
            {
                return await configurationService.GetUserProfileConfiguration2Async();
            }
            catch (Exception ex)
            {
                logger.Error("Error occurred while retrieving User Profile Configuration: " + ex.Message);
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Gets the Emergency Information Configuration
        /// </summary>
        /// <returns><see cref="EmergencyInformationConfiguration">Emergency Information Configuration</see></returns>
        [Obsolete("Obsolete as of API 1.16. Use version 2 of this API instead.")]
        public async Task<EmergencyInformationConfiguration> GetEmergencyInformationConfigurationAsync()
        {
            try
            {
                return await configurationService.GetEmergencyInformationConfigurationAsync();
            }
            catch (Exception ex)
            {
                logger.Error("Error occurred while retrieving Emergency Information Configuration: " + ex.Message);
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Gets the Emergency Information Configuration
        /// </summary>
        /// <returns><see cref="EmergencyInformationConfiguration2">Emergency Information Configuration</see></returns>
        public async Task<EmergencyInformationConfiguration2> GetEmergencyInformationConfiguration2Async()
        {
            try
            {
                return await configurationService.GetEmergencyInformationConfiguration2Async();
            }
            catch (Exception ex)
            {
                logger.Error("Error occurred while retrieving Emergency Information Configuration: " + ex.Message);
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Gets the Restriction Configuration
        /// </summary>
        /// <returns><see cref="RestrictionConfiguration">Restriction Configuration</see></returns>
        public async Task<RestrictionConfiguration> GetRestrictionConfigurationAsync()
        {
            try
            {
                return await configurationService.GetRestrictionConfigurationAsync();
            }
            catch (Exception ex)
            {
                logger.Error("Error occurred while retrieving Restriction Configuration: ", ex.Message);
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Gets the Privacy Configuration
        /// </summary>
        /// <returns><see cref="PrivacyConfiguration">Privacy Configuration</see></returns>
        public async Task<PrivacyConfiguration> GetPrivacyConfigurationAsync()
        {
            try
            {
                return await configurationService.GetPrivacyConfigurationAsync();
            }
            catch (Exception ex)
            {
                logger.Error("Error occurred while retrieving Privacy Configuration: ", ex.Message);
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Gets the Organizational Relationship Configuration
        /// </summary>
        /// <returns><see cref="OrganizationalRelationshipConfiguration">OrganizationalRelationship Configuration</see></returns>
        public async Task<OrganizationalRelationshipConfiguration> GetOrganizationalRelationshipConfigurationAsync()
        {
            try
            {
                return await configurationService.GetOrganizationalRelationshipConfigurationAsync();
            }
            catch (Exception ex)
            {
                logger.Error("Error occurred while retrieving OrganizationalRelationship Configuration: ", ex.Message);
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        #region backup configuration
        
        /// <summary>
        /// Writes a new configuration data record to Colleague 
        /// </summary>
        /// <param name="backupData">data to backup</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<BackupConfiguration> PostConfigBackupDataAsync([FromBody] BackupConfiguration backupData)
        {
            try
            {
                var result = await configurationService.WriteBackupConfigurationAsync(backupData);
                return result;
            }
            catch (PermissionsException ex)
            {
                logger.Error("Permission error occurred: ", ex.Message);
                throw CreateHttpResponseException("Permission error occurred.", HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                logger.Error("Error occurred while adding Backup Configuration: ", ex.Message);
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Lookup a configuration data record from Colleague
        /// </summary>
        /// <param name="backupDataQuery">Criteria for looking up backup config data.</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IEnumerable<BackupConfiguration>> QueryBackupConfigDataByPostAsync([FromBody] BackupConfigurationQueryCriteria backupDataQuery)
        {
            // must supply namespace + optional datetime filter
            /* e.g. POST /qapi/configuration/
             {
                "Namespace": "Ellucian/Colleague Web API/1.18.0.0/dvetk_wstst01_rt",
                "OnOrBeforeDateTimeUtc":"2017-10-22T02:57:22Z"
             }
             */
            if (backupDataQuery == null)
            {
                throw CreateHttpResponseException(
                    "Missing required query criteria.",
                    HttpStatusCode.BadRequest);
            }
            try
            {
                var result = await configurationService.ReadBackupConfigurationAsync(backupDataQuery);
                return result;
            }
            catch (PermissionsException ex)
            {
                logger.Error("Permission error occurred: ", ex.Message);
                throw CreateHttpResponseException("Permission error occurred.", HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                logger.Error("Error occurred while retrieving Backup Configuration: ", ex.Message);
                HttpStatusCode errorCode = HttpStatusCode.BadRequest;
                if (ex.Message.Contains("No backup record found"))
                {
                    errorCode = HttpStatusCode.NotFound;
                }
                throw CreateHttpResponseException(ex.Message, errorCode);
            }
        }

        /// <summary>
        /// Reads a configuration data record from Colleague
        /// </summary>
        /// <param name="id">ID or key of backup record</param>
        /// <returns></returns>
        public async Task<BackupConfiguration> GetConfigBackupDataAsync(string id)
        {
            // must supply either the id, e.g.
            // /configuration/0503be99-fd54-4152-939b-6743d00e0334
            if (string.IsNullOrWhiteSpace(id))
            {
                throw CreateHttpResponseException(
                    "Config data record ID must be specified.",
                    HttpStatusCode.BadRequest);
            }
            try
            {
                var query = new BackupConfigurationQueryCriteria()
                {
                    ConfigurationIds = new List<string>() { id },
                    Namespace = null
                };
                var result = await configurationService.ReadBackupConfigurationAsync(query);
                return result.FirstOrDefault();
            }
            catch (PermissionsException ex)
            {
                logger.Error("Permission error occurred: ", ex.Message);
                throw CreateHttpResponseException("Permission error occurred.", HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                logger.Error("Error occurred while retrieving Backup Configuration: ", ex.Message);
                HttpStatusCode errorCode = HttpStatusCode.BadRequest;
                if (ex.Message.Contains("No backup record found"))
                {
                    errorCode = HttpStatusCode.NotFound;
                }
                throw CreateHttpResponseException(ex.Message, errorCode);
            }
        }

        /// <summary>
        /// Causes this API instance to perform a backup of its configuration data.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task PostBackupApiConfigAsync()
        {
            try
            {
                await configurationService.BackupApiConfigurationAsync();

            }
            catch (PermissionsException ex)
            {
                logger.Error("Permission error occurred: ", ex.Message);
                throw CreateHttpResponseException("Permission error occurred", HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                logger.Error("Error occurred while backing up API configuration data: ", ex.Message);
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Causes this API instance to perform a restore of its configuration data using
        /// backup data retrieved from Colleague. Optionally merge the resx files.
        /// Also cause app pool to recycle.
        /// Note: the resx file merge operation currently is supported on a brand new instance only. It will
        /// not run on an instance whose resource files have already been modified.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task PostRestoreApiConfigAsync(DateTimeOffset? onOrBeforeDateTime = null)
        {
            try
            {
                var apiBackupData = await configurationService.RestoreApiBackupConfigurationAsync();

                // even though the service method will update web.config to restore the appSettings values,
                // we'll still ensure the app pool gets recycled by touching web.config again here.
                RecycleApp();
            }
            catch (PermissionsException ex)
            {
                logger.Error("Permission error occurred: ", ex.Message);
                throw CreateHttpResponseException("Permission error occurred", HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                logger.Error("Error occurred restoring this API instance's previous configuration data: ", ex.Message);
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        private void RecycleApp()
        {
            try
            {
                // Touch web.config to force a recycle
                string configPath = System.Web.HttpRuntime.AppDomainAppPath + "\\web.config";
                System.IO.File.SetLastWriteTimeUtc(configPath, DateTime.UtcNow);
            }
            catch (UnauthorizedAccessException uae)
            {
                logger.Error("Error occurred touching web.config to force a recycle ", uae.Message);
                throw new Exception("The settings were saved, but restarting the application failed. Either manually recycle the application pool, or verify that the web.config file " +
                        " is not read only and that the application pool has write permissions.", uae);
            }
        }

        #endregion

        /// <summary>
        /// Retrieves Colleague Self-Service configuration information
        /// </summary>
        /// <returns>A <see cref="SelfServiceConfiguration"/> object</returns>
        /// <accessComments>Any authenticated user can get this resource.</accessComments>
        [HttpGet]
        public async Task<SelfServiceConfiguration> GetSelfServiceConfigurationAsync()
        {
            try
            {
                return await configurationService.GetSelfServiceConfigurationAsync();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error occurred while retrieving Self-Service Configuration.");
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }
    }
}
