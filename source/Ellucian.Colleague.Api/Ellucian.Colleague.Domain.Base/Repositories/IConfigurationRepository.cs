// Copyright 2014-2022 Ellucian Company L.P. and its affiliates.

using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Entities;
using System;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Repositories
{
    /// <summary>
    /// Provides access to configuration data.
    /// </summary>
    public interface IConfigurationRepository : IEthosExtended
    {
        /// <summary>
        /// Contains a Tuple where Item1 is a bool set to true if any fields are denied or secured, 
        /// Item2 is a list of DeniedAccess Fields and Item3 is a list of Restricted fields.
        /// </summary>
        Tuple<bool, List<string>, List<string>> GetSecureDataDefinition();

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
        /// Gets the User profile configuration.
        /// </summary>
        /// <returns><see cref="UserProfileConfiguration"/>User Profile Configuration</returns>
        [Obsolete("This method services version of the API prior to 1.16.")]
        Task<UserProfileConfiguration> GetUserProfileConfigurationAsync();

        /// <summary>
        /// Gets the User profile configuration.
        /// </summary>
        /// <param name="allAdrelTypes">Address Relation Type codes</param>
        /// <returns><see cref="UserProfileConfiguration2"/>User Profile Configuration</returns>
        Task<UserProfileConfiguration2> GetUserProfileConfiguration2Async(List<AddressRelationType> allAdrelTypes);

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
        /// <param name="reportEthosApiErrors">Flag to determine if we should throw an exception on Extended Errors.</param>
        /// <param name="bypassCache">Flag to indicate if we should bypass the cache and read directly from disk.</param>
        /// <returns>List with all of the extended data if aavailable. Returns an empty list if none available or none configured</returns>
        Task<IEnumerable<EthosExtensibleData>> GetExtendedEthosDataByResource(string resourceName, string resourceVersionNumber, string extendedSchemaResourceId, IEnumerable<string> resournceIds,
            Dictionary<string, Dictionary<string, string>> allColumnData = null, bool reportEthosApiErrors = false, bool bypassCache = false, bool useRecordKey = false, bool returnRestrictedFields = false);

        ///// <summary>
        ///// Gets all of the Ethos Extensiblity settings stored on EDM.EXT.VERSIONS
        ///// </summary>
        ///// <param name="bypassCache">bool to determine if cache should be bypassed</param>
        ///// <returns>List of DataContracts.EdmExtVersions</returns>
        //Task<IEnumerable<EdmExtVersions>> GetEthosExtensibilityConfiguration(bool bypassCache = false);


        /// <summary>
        /// Gets the extended configuration available on a resource, returns null if there are none
        /// </summary>
        /// <param name="resourceName">name of the resource (api) </param>
        /// <param name="resourceVersionNumber">version number of ther resource</param>
        /// <param name="extendedSchemaResourceId">extended schema identifier</param>
        /// <returns> extended configuration if available. Returns null if none available or none configured</returns>
        Task<EthosExtensibleData> GetExtendedEthosConfigurationByResource(string resourceName, string resourceVersionNumber, string extendedSchemaResourceId, bool bypassCache = false, bool readRtFields = false);

        /// <summary>
        /// Return the default version for an extension used in Stand Alone API builder.
        /// </summary>
        /// <param name="resourceName"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        Task<string> GetEthosExtensibilityResourceDefaultVersion(string resourceName, bool bypassCache = false, string requestedVersion = "");

        /// <summary>
        /// Gets the extended configuration available on a resource, returns null if there are none
        /// </summary>
        /// <param name="resourceName">name of the resource (api) </param>
        /// <returns> extended configuration if available. Returns null if none available or none configured</returns>
        Task<EthosApiConfiguration> GetEthosApiConfigurationByResource(string resourceName, bool bypassCache = false);


        /// <summary>
        /// Gets all of the Ethos Extensiblity settings stored on EDM.EXT.VERSIONS
        /// </summary>
        /// <param name="bypassCache">bool to determine if cache should be bypassed</param>
        /// <returns>List of Domain.Base.Entities.EthosExtensibleData</returns>
        Task<IEnumerable<Domain.Base.Entities.EthosExtensibleData>> GetEthosExtensibilityConfigurationEntities(bool customOnly = true, bool bypassCache = false);

        /// <summary>
        /// Gets all of the Ethos Extensiblity settings stored on EDM.EXT.VERSIONS for a resource
        /// </summary>
        /// <param name="bypassCache">bool to determine if cache should be bypassed</param>
        /// <returns>List of Domain.Base.Entities.EthosExtensibleData</returns>
        Task<IEnumerable<Domain.Base.Entities.EthosExtensibleData>> GetEthosExtensibilityConfigurationEntitiesByResource(string resourceName, bool customOnly = true, bool bypassCache = false);

        /// <summary>
        /// Encode a primary key for use in Extensibility
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Encoded string to use as guid on a non-guid based API.</returns>
        string EncodePrimaryKey(string id);

        /// <summary>
        /// Un-Encode a primary key for use in Extensibility
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Un-Encoded string taken from a non-guid based API guid.</returns>
        string UnEncodePrimaryKey(string id);

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

        /// <summary>
        /// Gets the Required document configuration.
        /// </summary>
        /// <returns><see cref="RequiredDocumentConfiguration"/>Required Document Configuration</returns>
        Task<RequiredDocumentConfiguration> GetRequiredDocumentConfigurationAsync();

        /// <summary>
        /// Gets the Session Configuration.
        /// </summary>
        /// <returns>Session Configuration entity</returns>
        Task<SessionConfiguration> GetSessionConfigurationAsync();

        /// <summary>
        /// Gets the list of Audit Log configuration records from Colleague
        /// </summary>
        /// <returns>An <see cref="AuditLogConfiguration">Audit Log configuration</see></returns>
        Task<IEnumerable<AuditLogConfiguration>> GetAuditLogConfigurationAsync(bool bypassCache = false);

        /// <summary>
        /// Get a collection of AuditLogCategories
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of AuditLogCategories</returns>
        Task<IEnumerable<AuditLogCategory>> GetAuditLogCategoriesAsync(bool ignoreCache);

        /// <summary>
        /// Updates a single item in the list of Audit Log configuration records from Colleague
        /// </summary>
        /// <param name="auditLogConfiguration">Audit Log Configuration to update</param>
        /// <returns>An <see cref="AuditLogConfiguration">Updated Audit Log configuration</see></returns>
        Task<AuditLogConfiguration> UpdateAuditLogConfigurationAsync(AuditLogConfiguration auditLogConfiguration);


        ///////////////////////////////////////////////////////////////////////////////////
        ///                                                                             ///
        ///                               CF Team                                       ///                                                                             
        ///                         TAX INFORMATION VIEWS                               ///
        ///           TAX FORMS CONFIGURATION, CONSENTs, STATEMENTs, PDFs               ///
        ///                                                                             ///
        ///////////////////////////////////////////////////////////////////////////////////

        #region CF Views

        /// <summary>
        /// Gets the configuration consent paragraphs for tax forms.
        /// </summary>
        /// <param name="taxForm">The tax form (W-2, 1095-C, 1098-T, etc.)</param>
        /// <returns><see cref="TaxFormConfiguration2"/>Cconsent and withheld paragraphs for the tax form</see></returns>
        Task<TaxFormConfiguration2> GetTaxFormConsentConfiguration2Async(string taxForm);

        /// <summary>
        /// Gets the configuration availability dates for tax forms.
        /// </summary>
        /// <param name="taxForm">The tax form (W-2, 1095-C, 1098-T, etc.)</param>
        /// <returns>Availability dates for the tax form</returns>
        Task<TaxFormConfiguration2> GetTaxFormAvailabilityConfiguration2Async(string taxForm);


        #region OBSOLETE METHODS

        /// <summary>
        /// Gets the configuration consent paragraphs for tax forms.
        /// </summary>
        /// <param name="taxFormId">The tax form (W-2, 1095-C, 1098-T, etc.)</param>
        /// <returns><see cref="TaxFormConfiguration"/>Cconsent and withheld paragraphs for the tax form</see></returns>
        [Obsolete("Obsolete as of API 1.29.1. Use GetTaxFormConsentConfiguration2Async instead.")]
        Task<TaxFormConfiguration> GetTaxFormConsentConfigurationAsync(TaxForms taxFormId);

        /// <summary>
        /// Gets the configuration availability dates for tax forms.
        /// </summary>
        /// <param name="taxFormId">The tax form (W-2, 1095-C, 1098-T, etc.)</param>
        /// <returns>Availability dates for the tax form</returns>
        [Obsolete("Obsolete as of API 1.29.1. Use GetTaxFormAvailabilityConfiguration2Async instead.")]
        Task<TaxFormConfiguration> GetTaxFormAvailabilityConfigurationAsync(TaxForms taxFormId);
        

        #endregion

        #endregion
    }
}