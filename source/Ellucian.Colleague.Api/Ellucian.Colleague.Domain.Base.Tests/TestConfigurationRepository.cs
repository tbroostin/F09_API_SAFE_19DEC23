// Copyright 2015-2021 Ellucian Company L.P. and its affiliates.

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
        /// <summary>
        /// Dictionary of string, string that contains the Ethos Extended Data to send into the CTX
        /// key is column name
        /// value is value to save in, if empty string then this means it is meant to remove the data from colleague
        /// </summary>
        public Dictionary<string, string> EthosExtendedDataDictionary { get; set; }

        /// <summary>
        /// Takes the EthosExtendedDataList dictionary and splits it into two List string to be passed to Colleague CTX 
        /// </summary>
        /// <returns>T1 is the list of keys, T2 is a list values that match up. Returns null if the list is empty</returns>
        public Tuple<List<string>, List<string>> GetEthosExtendedDataLists()
        {
            return new Tuple<List<string>, List<string>>(new List<string>(), new List<string>());
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
            upc.UpdateAddressTypeConfiguration(true, null, null, null);
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
            return await Task.Run(() => new ProxyConfiguration(true, "DISCLOSURE.ID", "EMAIL.ID", true, true, new List<ProxyAndUserPermissionsMap>()));
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

        public async Task<IEnumerable<EthosExtensibleData>> GetExtendedEthosDataByResource(string resourceName, string resourceVersionNumber, string extendedSchemaResourceId, IEnumerable<string> resournceIds, bool reportEthosApiErrors = false, bool bypassCache = false, bool useRecordKey = false)
        {
            var ethosExtensibleData = new List<EthosExtensibleData>();

            foreach (var resourceId in resournceIds)
            {
                var ethosData = new EthosExtensibleData(resourceName, resourceVersionNumber, extendedSchemaResourceId, resourceId, "");

                var dataRow = new EthosExtensibleDataRow("LAST.NAME", "PERSON", "lastName", "/name/", "string", "Bennett", 35);
                ethosData.AddItemToExtendedData(dataRow);

                ethosExtensibleData.Add(ethosData);
            }

            return ethosExtensibleData;
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

        public async Task<EthosExtensibleData> GetExtendedEthosConfigurationByResource(string resourceName, string resourceVersionNumber, string extendedSchemaResourceId, bool bypassCache = false)
        {
            var dataRow = new EthosExtensibleDataRow("LAST.NAME", "PERSON", "lastName", "/name/", "string", "Bennett", 35);
            var dataRows = new List<EthosExtensibleDataRow>() { dataRow };
            var ethosExtensibleData = new EthosExtensibleData(resourceName, resourceVersionNumber, "", extendedSchemaResourceId, "", dataRows);

            return ethosExtensibleData;
        }

        public async Task<string> GetEthosExtensibilityResourceDefaultVersion(string resourceName, bool bypassCache = false)
        {
            return "1.0.0";
        }

        public async Task<EthosApiConfiguration> GetEthosApiConfigurationByResource(string resourceName, bool bypassCache = false)
        {
            var ethosApiConfiguration = new EthosApiConfiguration()
            {
                ResourceName = resourceName,
                PrimaryEntity = "PERSON",
                PrimaryGuidFileName = "PERSON",
                PrimaryGuidSource = "ID",
                PageLimit = 50,
                SelectFileName = "PERSON",
                SelectionCriteria = new List<EthosApiSelectCriteria>() { new EthosApiSelectCriteria("WITH", "PERSON.CORP.INDICATOR", "NE", "''") },
                HttpMethods = new List<EthosApiSupportedMethods>() {
                    new EthosApiSupportedMethods("GET", "VIEW.ANY.PERSON"),
                    new EthosApiSupportedMethods("POST", "UPDATE.PERSON"),
                    new EthosApiSupportedMethods("PUT", "UPDATE.PERSON"),
                    new EthosApiSupportedMethods("DELETE", "DELETE.PERSON.CONTACT")
                }
            };

            return ethosApiConfiguration;
        }

        /// <summary>
        /// Encode a primary key for use in Extensibility
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Encoded string to use as guid on a non-guid based API.</returns>
        public string EncodePrimaryKey(string id)
        {
            // Preserve all lower case and dashes in original key by manually escaping those characters.
            var returnData = Uri.EscapeDataString(id).Replace("-", "%2D").Replace("a", "%61").
                Replace("b", "%62").Replace("c", "%63").Replace("d", "%64").
                Replace("e", "%65").Replace("f", "%66").Replace("g", "%67").
                Replace("h", "%68").Replace("i", "%69").Replace("j", "%6a").
                Replace("k", "%6b").Replace("l", "%6c").Replace("m", "%6d").
                Replace("n", "%6e").Replace("o", "%6f").Replace("p", "%70").
                Replace("q", "%71").Replace("r", "%72").Replace("s", "%73").
                Replace("t", "%74").Replace("u", "%75").Replace("v", "%76").
                Replace("w", "%77").Replace("x", "%78").Replace("y", "%79").
                Replace("z", "%7a");
            return returnData.Replace("%", "-").ToLower();
        }

        /// <summary>
        /// Un-Encode a primary key for use in Extensibility
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Un-Encoded string taken from a non-guid based API guid.</returns>
        public string UnEncodePrimaryKey(string id)
        {
            var primaryKey = id.Replace("-", "%").ToUpper();
            return Uri.UnescapeDataString(primaryKey);
        }

        public async Task<SelfServiceConfiguration> GetSelfServiceConfigurationAsync()
        {
            return new SelfServiceConfiguration(true);
        }

        public async Task<RequiredDocumentConfiguration> GetRequiredDocumentConfigurationAsync()
        {
            var rdc = new RequiredDocumentConfiguration() { SuppressInstance = true, PrimarySortField = WebSortField.Status, SecondarySortField = WebSortField.OfficeDescription, TextForBlankStatus = "Blank status text", TextForBlankDueDate = "Blank due date text" };
            return rdc;
        }

        public async Task<UserProfileConfiguration2> GetUserProfileConfiguration2Async(List<AddressRelationType> webAdrelTypes)
        {
            var upc = new UserProfileConfiguration2();
            upc.UpdateAddressTypeConfiguration(true, new List<string>(), new List<string>());
            upc.UpdateEmailTypeConfiguration(false, new List<string>() { "COL" }, false, new List<string>() { "COL" });
            upc.UpdatePhoneTypeConfiguration(false, new List<string>() { "CP" }, new List<string>() { "CP" }, false);
            upc.CanUpdateAddressWithoutPermission = true;
            upc.CanUpdateEmailWithoutPermission = true;
            upc.CanUpdatePhoneWithoutPermission = true;
            upc.CanViewUpdateChosenName = UserProfileViewUpdateOption.Viewable;
            upc.CanViewUpdateGenderIdentity = UserProfileViewUpdateOption.Viewable;
            upc.CanViewUpdateNickname = UserProfileViewUpdateOption.Viewable;
            upc.CanViewUpdatePronoun = UserProfileViewUpdateOption.Viewable;
            upc.Text = "Test User Profile Configuration Text";
            return upc;
        }

        public async Task<SessionConfiguration> GetSessionConfigurationAsync()
        {
            return new SessionConfiguration();
        }

        public Task<IEnumerable<EthosExtensibleData>> GetEthosExtensibilityConfigurationEntities(bool customOnly = true, bool bypassCache = false)
        {
            throw new NotImplementedException();
        }



        ///////////////////////////////////////////////////////////////////////////////////
        ///                                                                             ///
        ///                               CF Team                                       ///                                                                             
        ///                          TAX INFORMATION VIEWS                              ///
        ///                         TAX FORMS CONFIGURATION                             ///
        ///                                                                             ///
        ///////////////////////////////////////////////////////////////////////////////////


        public List<TaxFormConfiguration> TaxFormConfigurations = new List<TaxFormConfiguration>();
        public List<TaxFormConfiguration2> TaxFormConfigurations2 = new List<TaxFormConfiguration2>();

        public TestConfigurationRepository()
        {
            this.TaxFormConfigurations2.Add(new TaxFormConfiguration2(TaxFormTypes.FormW2)
            {
                ConsentParagraphs = new TaxFormConsentParagraph()
                {
                    ConsentText = "I consent to receive my W-2 online, version 2.",
                    ConsentWithheldText = "I withhold to receiving my W-2 online, version 2."
                }
            });

            this.TaxFormConfigurations2.Add(new TaxFormConfiguration2(TaxFormTypes.Form1095C)
            {
                ConsentParagraphs = new TaxFormConsentParagraph()
                {
                    ConsentText = "I consent to receive my 1095-C online, version 2.",
                    ConsentWithheldText = "I withhold to receiving my 1095-C online, version 2."
                }
            });

            this.TaxFormConfigurations2.Add(new TaxFormConfiguration2(TaxFormTypes.Form1098)
            {
                ConsentParagraphs = new TaxFormConsentParagraph()
                {
                    ConsentText = "I consent to receive my 1098 online, version 2.",
                    ConsentWithheldText = "I withhold to receiving my 1098 online, version 2."
                }
            });

            this.TaxFormConfigurations2.Add(new TaxFormConfiguration2(TaxFormTypes.FormT4)
            {
                HideConsent = true,
                ConsentParagraphs = new TaxFormConsentParagraph()
                {
                    ConsentText = "I consent to receive my T4 online, version 2.",
                    ConsentWithheldText = "I withhold to receiving my T4 online, version 2."
                }
            }); ;

            this.TaxFormConfigurations2.Add(new TaxFormConfiguration2(TaxFormTypes.FormT4A)
            {
                HideConsent = true,
                ConsentParagraphs = new TaxFormConsentParagraph()
                {
                    ConsentText = "I consent to receive my T4A online, version 2.",
                    ConsentWithheldText = "I withhold to receiving my T4A online, version 2."
                }
            });

            this.TaxFormConfigurations2.Add(new TaxFormConfiguration2(TaxFormTypes.FormT2202A)
            {
                HideConsent = true,
                ConsentParagraphs = new TaxFormConsentParagraph()
                {
                    ConsentText = "I consent to receive my T2202A online, version 2.",
                    ConsentWithheldText = "I withhold to receiving my T2202A online, version 2."
                }
            });

            this.TaxFormConfigurations2.Add(new TaxFormConfiguration2(TaxFormTypes.Form1099MI)
            {
                ConsentParagraphs = new TaxFormConsentParagraph()
                {
                    ConsentText = "I consent to receive my 1099-MISC online, version 2.",
                    ConsentWithheldText = "I withhold to receiving my 1099-MISC online, version 2."
                }
            });

            this.TaxFormConfigurations2.Add(new TaxFormConfiguration2(TaxFormTypes.Form1099NEC)
            {
                ConsentParagraphs = new TaxFormConsentParagraph()
                {
                    ConsentText = "I consent to receive my 1099-NEC online, version 2.",
                    ConsentWithheldText = "I withhold to receiving my 1099-NEC online, version 2."
                }
            });


            this.TaxFormConfigurations.Add(new TaxFormConfiguration(TaxForms.FormW2)
            {
                ConsentParagraphs = new TaxFormConsentParagraph()
                {
                    ConsentText = "I consent to receive my W-2 online.",
                    ConsentWithheldText = "I withhold to receiving my W-2 online."
                }
            });

            this.TaxFormConfigurations.Add(new TaxFormConfiguration(TaxForms.Form1095C)
            {
                ConsentParagraphs = new TaxFormConsentParagraph()
                {
                    ConsentText = "I consent to receive my 1095-C online.",
                    ConsentWithheldText = "I withhold to receiving my 1095-C online."
                }
            });

            this.TaxFormConfigurations.Add(new TaxFormConfiguration(TaxForms.Form1098)
            {
                ConsentParagraphs = new TaxFormConsentParagraph()
                {
                    ConsentText = "I consent to receive my 1098 online.",
                    ConsentWithheldText = "I withhold to receiving my 1098 online."
                }
            });

            this.TaxFormConfigurations.Add(new TaxFormConfiguration(TaxForms.FormT4)
            {
                ConsentParagraphs = new TaxFormConsentParagraph()
                {
                    ConsentText = "I consent to receive my T4 online.",
                    ConsentWithheldText = "I withhold to receiving my T4 online."
                }
            });

            this.TaxFormConfigurations.Add(new TaxFormConfiguration(TaxForms.FormT4A)
            {
                ConsentParagraphs = new TaxFormConsentParagraph()
                {
                    ConsentText = "I consent to receive my T4A online.",
                    ConsentWithheldText = "I withhold to receiving my T4A online."
                }
            });

            this.TaxFormConfigurations.Add(new TaxFormConfiguration(TaxForms.FormT2202A)
            {
                ConsentParagraphs = new TaxFormConsentParagraph()
                {
                    ConsentText = "I consent to receive my T2202A online.",
                    ConsentWithheldText = "I withhold to receiving my T2202A online."
                }
            });

            this.TaxFormConfigurations.Add(new TaxFormConfiguration(TaxForms.Form1099MI)
            {
                ConsentParagraphs = new TaxFormConsentParagraph()
                {
                    ConsentText = "I consent to receive my 1099MI online.",
                    ConsentWithheldText = "I withhold to receiving my 1099MI online."
                }
            });
        }

        public async Task<TaxFormConfiguration2> GetTaxFormConsentConfiguration2Async(string taxForm)
        {
            return await Task.Run(() => this.TaxFormConfigurations2.Where(x => x.TaxForm == taxForm).FirstOrDefault());
        }

        public async Task<TaxFormConfiguration2> GetTaxFormAvailabilityConfiguration2Async(string taxForm)
        {
            return await Task.Run(() => this.TaxFormConfigurations2.FirstOrDefault(x => x.TaxForm == taxForm));
        }

        // Obsolete
        public async Task<TaxFormConfiguration> GetTaxFormConsentConfigurationAsync(TaxForms taxFormId)
        {
            return await Task.Run(() => this.TaxFormConfigurations.Where(x => x.TaxFormId == taxFormId).FirstOrDefault());
        }

        // Obsolete
        public async Task<TaxFormConfiguration> GetTaxFormAvailabilityConfigurationAsync(TaxForms taxFormId)
        {
            return await Task.Run(() => this.TaxFormConfigurations.FirstOrDefault(x => x.TaxFormId == taxFormId));
        }
    }
}
