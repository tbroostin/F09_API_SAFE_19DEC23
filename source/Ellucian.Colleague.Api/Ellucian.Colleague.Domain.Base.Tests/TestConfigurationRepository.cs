// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using System;

namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestConfigurationRepository : IConfigurationRepository
    {
        public List<TaxFormConfiguration> TaxFormConfigurations = new List<TaxFormConfiguration>();

        public TestConfigurationRepository()
        {
            this.TaxFormConfigurations.Add(new TaxFormConfiguration(TaxForms.FormW2)
            {
                ConsentParagraphs = new TaxFormConsentParagraph()
                {
                    ConsentText = "In consent to receive my W-2 online.",
                    ConsentWithheldText = "I withhold to receiving my W-2 online."
                }
            });

            this.TaxFormConfigurations.Add(new TaxFormConfiguration(TaxForms.Form1095C)
            {
                ConsentParagraphs = new TaxFormConsentParagraph()
                {
                    ConsentText = "In consent to receive my 1095-C online.",
                    ConsentWithheldText = "I withhold to receiving my 1095-C online."
                }
            });

            this.TaxFormConfigurations.Add(new TaxFormConfiguration(TaxForms.Form1098)
            {
                ConsentParagraphs = new TaxFormConsentParagraph()
                {
                    ConsentText = "In consent to receive my 1098 online.",
                    ConsentWithheldText = "I withhold to receiving my 1098 online."
                }
            });

            this.TaxFormConfigurations.Add(new TaxFormConfiguration(TaxForms.FormT4)
            {
                ConsentParagraphs = new TaxFormConsentParagraph()
                {
                    ConsentText = "In consent to receive my T4 online.",
                    ConsentWithheldText = "I withhold to receiving my T4 online."
                }
            });

            this.TaxFormConfigurations.Add(new TaxFormConfiguration(TaxForms.FormT4A)
            {
                ConsentParagraphs = new TaxFormConsentParagraph()
                {
                    ConsentText = "In consent to receive my T4A online.",
                    ConsentWithheldText = "I withhold to receiving my T4A online."
                }
            });

            this.TaxFormConfigurations.Add(new TaxFormConfiguration(TaxForms.FormT2202A)
            {
                ConsentParagraphs = new TaxFormConsentParagraph()
                {
                    ConsentText = "In consent to receive my T2202A online.",
                    ConsentWithheldText = "I withhold to receiving my T2202A online."
                }
            });

            this.TaxFormConfigurations.Add(new TaxFormConfiguration(TaxForms.Form1099MI)
            {
                ConsentParagraphs = new TaxFormConsentParagraph()
                {
                    ConsentText = "In consent to receive my 1099MI online.",
                    ConsentWithheldText = "I withhold to receiving my 1099MI online."
                }
            });
        }

        public ExternalMapping GetExternalMapping(string mappingId)
        {
            return new ExternalMapping("mappingCode", "mappingDescription");
        }

        public DefaultsConfiguration GetDefaultsConfiguration()
        {
            return new DefaultsConfiguration();
        }

        public async Task<IntegrationConfiguration> GetIntegrationConfiguration(string integrationConfigurationId)
        {
            return new IntegrationConfiguration("id", "description", "url", true,
                12, "username", "password", "exchangeName", "queueName", "outExchangeName",
                "inExchangeName", "inExchangeQueueName", "apiUsername", "apiPassword", "erpName",
                AdapterDebugLevel.Debug, 10, new List<ResourceBusinessEventMapping>());
        }

        public async Task<TaxFormConfiguration> GetTaxFormConsentConfigurationAsync(TaxForms taxFormId)
        {
            return await Task.Run(() => this.TaxFormConfigurations.Where(x => x.TaxFormId == taxFormId).FirstOrDefault());
        }

        public async Task<TaxFormConfiguration> GetTaxFormAvailabilityConfigurationAsync(TaxForms taxFormId)
        {
            return await Task.Run(() => this.TaxFormConfigurations.FirstOrDefault(x => x.TaxFormId == taxFormId));
        }


        public async Task<UserProfileConfiguration> GetUserProfileConfigurationAsync()
        {
            var upc = new UserProfileConfiguration()
            {
                CanUpdateAddressWithoutPermission = true,
                CanUpdateEmailWithoutPermission = true,
                CanUpdatePhoneWithoutPermission = true,
                Text = "Test User Profile Configuration Text"
            };
            upc.UpdateAddressTypeConfiguration(true, null, true, "WB");
            upc.UpdateEmailTypeConfiguration(false, new List<string>() { "COL" }, false, new List<string>() { "COL" });
            upc.UpdatePhoneTypeConfiguration(false, new List<string>() { "CP" }, new List<string>() { "CP" });
            return upc;
        }

        public async Task<UserProfileConfiguration2> GetUserProfileConfiguration2Async()
        {
            var upc = new UserProfileConfiguration2()
            {
                CanUpdateAddressWithoutPermission = true,
                CanUpdateEmailWithoutPermission = true,
                CanUpdatePhoneWithoutPermission = true,
                Text = "Test User Profile Configuration Text"
            };
            upc.UpdateAddressTypeConfiguration(true, null, null);
            upc.UpdateEmailTypeConfiguration(false, new List<string>() { "COL" }, false, new List<string>() { "COL" });
            upc.UpdatePhoneTypeConfiguration(false, new List<string>() { "CP" }, new List<string>() { "CP" }, false);
            return upc;
        }

        public async Task<EmergencyInformationConfiguration> GetEmergencyInformationConfigurationAsync()
        {
            return new EmergencyInformationConfiguration(false, false, false, false);
        }

        public async Task<ProxyConfiguration> GetProxyConfigurationAsync()
        {
            return await Task.Run(() => new ProxyConfiguration(true, "DISCLOSURE.ID", "EMAIL.ID", true, true));
        }

        public async Task<RestrictionConfiguration> GetRestrictionConfigurationAsync()
        {
            var rc = new RestrictionConfiguration();
            rc.AddItem(new SeverityStyleMapping(0, 100, AlertStyle.Critical));
            return await Task.Run(() => rc);
        }

        public async Task<PilotConfiguration> GetPilotConfigurationAsync()
        {
            return null;
        }

        public async Task<PrivacyConfiguration> GetPrivacyConfigurationAsync()
        {
            return new PrivacyConfiguration("Record not accesible due to a privacy request");
        }

        public async Task<OrganizationalRelationshipConfiguration> GetOrganizationalRelationshipConfigurationAsync()
        {
            var config = new OrganizationalRelationshipConfiguration();
            config.RelationshipTypeCodeMapping.Add(OrganizationalRelationshipType.Manager, new List<string> { "MGR" });
            return config;
        }


        public async Task<IEnumerable<Ellucian.Colleague.Domain.Base.Entities.EthosSecurity>> GetEthosDataPrivacyConfiguration(bool bypassCache)
        {
            return null;
        }

        public Task<IEnumerable<EthosExtensibleData>> GetExtendedEthosDataByResource(string resourceName, string resourceVersionNumber, string extendedSchemaResourceId, IEnumerable<string> resournceIds, bool bypassCache = false)
        {
            return null;
        }


        public async Task<bool> IsThisTheEmaUser(string userName, bool bypassCache)
        {
            return false;
        }

        public async Task<BackupConfiguration> AddBackupConfigurationAsync(BackupConfiguration backupConfigToAdd)
        {
            return null;
        }
        
        public async Task<List<BackupConfiguration>> GetBackupConfigurationByIdsAsync(List<string> configDataId)
        {
            return null;
        }

        public async Task<List<BackupConfiguration>> GetBackupConfigurationByNamespaceAsync(string nameSpace)
        {
            return null;
        }

        public async Task<EthosExtensibleData> GetExtendedEthosConfigurationByResource(string resourceName, string resourceVersionNumber, string extendedSchemaResourceId)
        {
            return null;
        }

        public async Task<SelfServiceConfiguration> GetSelfServiceConfigurationAsync()
        {
            return new SelfServiceConfiguration(true);
        }
    }
}
