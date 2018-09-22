// Copyright 2014-2018 Ellucian Company L.P. and its affiliates.
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Entities;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Base.Repositories
{
    /// <summary>
    /// Provides access to configuration data.
    /// </summary>
    public interface IConfigurationRepository
    {
        /// <summary>
        /// Get the external mapping.
        /// </summary>
        /// <param name="mappingId">The ID of the mapping entity to get.</param>
        /// <returns><see cref="ExternalMapping">External mapping</see> entity</returns>
        ExternalMapping GetExternalMapping(string mappingId);

        /// <summary>
        /// Get the defaults configuration.
        /// </summary>
        /// <returns>The <see cref="DefaultsConfiguration">defaults configuration</see></returns>
        DefaultsConfiguration GetDefaultsConfiguration();

        /// <summary>
        /// Gets an integration configuration
        /// </summary>
        /// <param name="integrationConfigurationId">Integration Configuration ID</param>
        /// <returns>An <see cref="IntegrationConfiguration">integration configuration</see></returns>
        Task<IntegrationConfiguration> GetIntegrationConfiguration(string integrationConfigurationId);

        /// <summary>
        /// Gets the configuration consent paragraphs for tax forms.
        /// </summary>
        /// <param name="taxFormId">The tax form (W-2, 1095-C, 1098-T, etc.)</param>
        /// <returns><see cref="TaxFormConfiguration"/>Cconsent and withheld paragraphs for the tax form</see></returns>
        Task<TaxFormConfiguration> GetTaxFormConsentConfigurationAsync(TaxForms taxFormId);

        /// <summary>
        /// Gets the configuration availability dates for tax forms.
        /// </summary>
        /// <param name="taxFormId">The tax form (W-2, 1095-C, 1098-T, etc.)</param>
        /// <returns>Availability dates for the tax form</returns>
        Task<TaxFormConfiguration> GetTaxFormAvailabilityConfigurationAsync(TaxForms taxFormId);

        /// <summary>
        /// Gets the User profile configuration.
        /// </summary>
        /// <returns><see cref="UserProfileConfiguration"/>User Profile Configuration</returns>
        [Obsolete("This method services version of the API prior to 1.16.")]
        Task<UserProfileConfiguration> GetUserProfileConfigurationAsync();

        /// <summary>
        /// Gets the User profile configuration.
        /// </summary>
        /// <returns><see cref="UserProfileConfiguration2"/>User Profile Configuration</returns>
        Task<UserProfileConfiguration2> GetUserProfileConfiguration2Async();

        /// <summary>
        /// Gets the Emergency Information configuration.
        /// </summary>
        /// <returns><see cref="EmergencyInformationConfiguration"/>Emergency Information Configuration</returns>
        Task<EmergencyInformationConfiguration> GetEmergencyInformationConfigurationAsync();

        /// <summary>
        /// Gets the Restriction (Styling) Configuration
        /// </summary>
        /// <returns>Restriction Configuration</returns>
        Task<RestrictionConfiguration> GetRestrictionConfigurationAsync();

        /// <summary>
        /// Gets the Pilot parameter configuration.
        /// </summary>
        /// <returns><see cref="PilotParameters"/>Pilot Parameters</returns>
        Task<PilotConfiguration> GetPilotConfigurationAsync();

        /// <summary>
        /// Retrieves privacy configuration
        /// </summary>
        /// <returns>Privacy configuration</returns>
        Task<PrivacyConfiguration> GetPrivacyConfigurationAsync();

        /// <summary>
        /// Retrieves organizational relationship configuration
        /// </summary>
        /// <returns>Organizational relationship configuration</returns>
        Task<OrganizationalRelationshipConfiguration> GetOrganizationalRelationshipConfigurationAsync();

        /// <summary>
        /// Gets all of the EthosDataPrivacy settings stored on ETHOS.SECURITY accessed on form EDPS
        /// </summary>
        /// <param name="bypassCache">bool to determine if cache should be bypassed</param>
        /// <returns>List of Ellucian.Colleague.Domain.Base.Entities.EthosSecurity</returns>
        Task<IEnumerable<EthosSecurity>> GetEthosDataPrivacyConfiguration(bool bypassCache);

        /// <summary>
        /// Gets the extended data available on a resource, returns an empty list if there are no 
        /// </summary>
        /// <param name="resourceName">name of the resource (api) </param>
        /// <param name="resourceVersionNumber">version number of ther resource</param>
        /// <param name="extendedSchemaResourceId">extended schema identifier</param>
        /// <param name="resournceIds">IEnumerable of the ids for the resources in guid form</param>
        /// <returns>List with all of the extended data if aavailable. Returns an empty list if none available or none configured</returns>
        Task<IEnumerable<EthosExtensibleData>> GetExtendedEthosDataByResource(string resourceName, string resourceVersionNumber, string extendedSchemaResourceId, IEnumerable<string> resournceIds, bool bypassCache = false);

        /// <summary>
        /// Gets the extended configuration available on a resource, returns null if there are none
        /// </summary>
        /// <param name="resourceName">name of the resource (api) </param>
        /// <param name="resourceVersionNumber">version number of ther resource</param>
        /// <param name="extendedSchemaResourceId">extended schema identifier</param>
        /// <returns> extended configuration if available. Returns null if none available or none configured</returns>
        Task<EthosExtensibleData> GetExtendedEthosConfigurationByResource(string resourceName, string resourceVersionNumber, string extendedSchemaResourceId);

        /// <summary>
        /// Checks if the user making the API call is the EMA user based on the user settings on the EMA configuration
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="bypassCache"></param>
        /// <returns>True if EMA user, false if not</returns>
        Task<bool> IsThisTheEmaUser(string userName, bool bypassCache);
        
        /// <summary>
        /// Add/submit a new backup configuration record to the Colleague DB
        /// </summary>
        /// <param name="backupConfigToAdd"></param>
        /// <returns></returns>
        Task<BackupConfiguration> AddBackupConfigurationAsync(BackupConfiguration backupConfigToAdd);

        /// <summary>
        /// Retrieve backup configuration records from the Colleague DB with the
        /// specified input IDs.
        /// </summary>
        /// <param name="configDataIds"></param>
        /// <returns></returns>
        Task<List<BackupConfiguration>> GetBackupConfigurationByIdsAsync(List<string> configDataIds);

        /// <summary>
        /// Retrieve backup configuration records from the Colleague DB with the
        /// specified input namespace.
        /// </summary>
        /// <param name="nameSpace"></param>
        /// <returns></returns>
        Task<List<BackupConfiguration>> GetBackupConfigurationByNamespaceAsync(string nameSpace);

        /// <summary>
        /// Retrieves <see cref="SelfServiceConfiguration"/>
        /// </summary>
        /// <returns>A <see cref="SelfServiceConfiguration"/> object</returns>
        Task<SelfServiceConfiguration> GetSelfServiceConfigurationAsync();
    }
}