// Copyright 2014-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Http.Configuration;
using System.Collections.Generic;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    public interface IConfigurationService
    {
        /// <summary>
        /// Gets an integration configuration
        /// </summary>
        /// <param name="id">Integration Configuration ID</param>
        /// <returns>An integration configuration</returns>
        Task<Dtos.Base.IntegrationConfiguration> GetIntegrationConfiguration(string id);

        /// <summary>
        /// Returns the tax form configuration DTO for the tax form passed in.
        /// </summary>
        /// <param name="taxFormId">The tax form (W-2, 1095-C, 1098-T, etc.)</param>
        /// <returns>Tax form configuration DTO.</returns>
        Task<TaxFormConfiguration> GetTaxFormConsentConfigurationAsync(TaxForms taxFormId);

        /// <summary>
        /// Returns the user profile configuration
        /// </summary>
        /// <returns>User Profile Configuration dto</returns>
        [Obsolete("Obsolete as of API 1.16. Use GetUserProfileConfiguration2Async instead.")]
        Task<UserProfileConfiguration> GetUserProfileConfigurationAsync();

        /// <summary>
        /// Returns the user profile configuration
        /// </summary>
        /// <returns>User Profile Configuration dto</returns>
        Task<UserProfileConfiguration2> GetUserProfileConfiguration2Async();

        /// <summary>
        /// Returns the emergency information configuration
        /// </summary>
        /// <returns>Emergency Information Configuration dto</returns>
        [Obsolete("Obsolete as of API 1.16. Use GetEmergencyInformationConfiguration2Async instead.")]
        Task<EmergencyInformationConfiguration> GetEmergencyInformationConfigurationAsync();

        /// <summary>
        /// Returns the emergency information configuration
        /// </summary>
        /// <returns>Emergency Information Configuration dto</returns>
        Task<EmergencyInformationConfiguration2> GetEmergencyInformationConfiguration2Async();

        /// <summary>
        /// Returns the restriction configuration
        /// </summary>
        /// <returns>Restriction Configuration DTO</returns>
        Task<RestrictionConfiguration> GetRestrictionConfigurationAsync();

        /// <summary>
        /// Returns the privacy configuration
        /// </summary>
        /// <returns>Privacy Configuration DTO</returns>
        Task<PrivacyConfiguration> GetPrivacyConfigurationAsync();

        /// <summary>
        /// Retrieves the organizational relationship configuration
        /// </summary>
        /// <returns>Organizational relationship configuration DTO</returns>
        Task<OrganizationalRelationshipConfiguration> GetOrganizationalRelationshipConfigurationAsync();


        /// <summary>
        /// Stores the supplied backup config data to the Colleague DB
        /// </summary>
        /// <param name="backupConfiguration"></param>
        /// <returns></returns>
        Task<Ellucian.Colleague.Dtos.Base.BackupConfiguration> WriteBackupConfigurationAsync(
            Ellucian.Colleague.Dtos.Base.BackupConfiguration backupConfiguration);

        /// <summary>
        /// Retrieve the latest backup config data of the given namespace from the Colleague DB. An optional
        /// date time filter can be used.
        /// </summary>
        /// <param name="nameSpace"></param>
        /// <returns></returns>
        Task<IEnumerable<Ellucian.Colleague.Dtos.Base.BackupConfiguration>> ReadBackupConfigurationAsync(BackupConfigurationQueryCriteria criteria);

        /// <summary>
        /// Backs up this API instance's config data to the Colleague DB
        /// </summary>
        /// <returns></returns>
        Task BackupApiConfigurationAsync();

        /// <summary>
        /// Restores this API instance's config data using the latest backup retrieved from Colleague DB.
        /// An optional date time filter can be used.
        /// </summary>
        /// <returns></returns>
        Task<ApiBackupConfigData> RestoreApiBackupConfigurationAsync();

        /// <summary>
        /// Retrieves <see cref="SelfServiceConfiguration"/>
        /// </summary>
        /// <returns>A <see cref="SelfServiceConfiguration"/> object</returns>
        Task<SelfServiceConfiguration> GetSelfServiceConfigurationAsync();

        /// <summary>
        /// Returns the required document configuration
        /// </summary>
        /// <returns>Required Document Configuration dto</returns>
        Task<RequiredDocumentConfiguration> GetRequiredDocumentConfigurationAsync();
    }
}