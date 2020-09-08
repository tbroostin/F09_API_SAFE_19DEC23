// Copyright 2014-2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Base.Tests.Repositories
{
    [TestClass]
    public class ConfigurationRepositoryTests : BaseRepositorySetup
    {
        #region Initialize and Cleanup

        ConfigurationRepository ConfigRepository;
        static Collection<CdmIntegration> cdmIntegrationResponses = TestCdmIntegrationRepository.CdmIntegrationResponses;
        static Collection<ElfTranslateTables> elfTranslateTables = TestElfTranslateTablesRepository.ElfTranslateTables;

        ApiSettings FakeApiSettingsWithGmtColleagueTimeZone;

        BackupConfiguration FakeBackupConfigurationData;
        Dictionary<string, GuidLookupResult> guidLookupResults;

        ReadBackupConfigDataResponse FakeBackupConfigReadResponse;
        BackupConfigRecords FakeBackupConfigReadResponseRecord;

        private HrwebDefaults expectedHrWebDefaults = new HrwebDefaults()
        {
            HrwebW2oConText = "I consent to receive my W-2 online.",
            HrwebW2oWhldText = "I withhold consent to receive my W-2 online.",

            Hrweb1095cConText = "I consent to receive my 1095-C online.",
            Hrweb1095cWhldText = "I withhold consent to receive my 1095-C online."
        };

        private Parm1098 parm1098DataContract = new Parm1098()
        {
            P1098ConsentText = "I consent to receive my 1098 online.",
            P1098WhldConsentText = "I withhold consent to receive my 1098 online.",
            P1098TTaxForm = "1098T",
            P1098ETaxForm = "1098E"
        };

        private ParmT4 parmT4DataContract = new ParmT4()
        {
            Pt4ConText = "I consent to receive my T4 online.",
            Pt4WhldText = "I withhold consent to receive my T4 online.",
        };

        private ParmT4a parmT4ADataContract = new ParmT4a()
        {
            Pt4aConText = "I consent to receive my T4A online.",
            Pt4aWhldText = "I withhold consent to receive my T4A online."
        };

        private CnstRptParms cnstRptParmsContract = new CnstRptParms()
        {
            CnstConsentText = "I consent to receive my T2202A online.",
            CnstWhldConsentText = "I withhold consent to receive my T2202A online.",

            CnstT2202aPdfParmsEntityAssociation = new List<CnstRptParmsCnstT2202aPdfParms>()
            {
                new CnstRptParmsCnstT2202aPdfParms()
                {
                    CnstT2202aPdfTaxYearAssocMember = "2010",
                    CnstT2202aPdfWebFlagAssocMember = "Y"
                },
                new CnstRptParmsCnstT2202aPdfParms()
                {
                    CnstT2202aPdfTaxYearAssocMember = "2011",
                    CnstT2202aPdfWebFlagAssocMember = "Y"
                },
                new CnstRptParmsCnstT2202aPdfParms()
                {
                    CnstT2202aPdfTaxYearAssocMember = "2012",
                    CnstT2202aPdfWebFlagAssocMember = "Y"
                },
                new CnstRptParmsCnstT2202aPdfParms()
                {
                    CnstT2202aPdfTaxYearAssocMember = "2013",
                    CnstT2202aPdfWebFlagAssocMember = "Y"
                },
                new CnstRptParmsCnstT2202aPdfParms()
                {
                    CnstT2202aPdfTaxYearAssocMember = "2014",
                    CnstT2202aPdfWebFlagAssocMember = "Y"
                }
            }
        };

        private CorewebDefaults corewebDefaults;
        private Dflts dflts;

        private QtdYtdParameterW2 expectedQtdYtdParameterW2 = new QtdYtdParameterW2()
        {
            QypWebW2Years = new List<string>() { "2010", "2011", "2012", "2013", "2014" },
            QypWebW2AvailableDates = new List<DateTime?>()
            {
                new DateTime(2011, 01, 24),
                new DateTime(2012, 01, 26),
                new DateTime(2013, 02, 03),
                new DateTime(2014, 01, 16),
                new DateTime(2015, 01, 20),
            }
        };

        private QtdYtdParameter1095C expectedQtdYtdParameter1095C = new QtdYtdParameter1095C()
        {
            QypWeb1095cYears = new List<string>() { "2010", "2011", "2012", "2013", "2014" },
            QypWeb1095cAvailDates = new List<DateTime?>()
            {
                new DateTime(2011, 01, 24),
                new DateTime(2012, 01, 26),
                new DateTime(2013, 02, 03),
                new DateTime(2014, 01, 16),
                new DateTime(2015, 01, 20),
            }
        };

        private QtdYtdParameterT4 expectedQtdYtdParameterT4 = new QtdYtdParameterT4()
        {
            WebT4ParameterEntityAssociation = new List<QtdYtdParameterT4WebT4Parameter>()
            {
                new QtdYtdParameterT4WebT4Parameter()
                {
                    QypWebT4AvailableDatesAssocMember = new DateTime(2011, 01, 24),
                    QypWebT4YearsAssocMember = "2010"
                },
                new QtdYtdParameterT4WebT4Parameter()
                {
                    QypWebT4AvailableDatesAssocMember = new DateTime(2012, 01, 26),
                    QypWebT4YearsAssocMember = "2011"
                },
                new QtdYtdParameterT4WebT4Parameter()
                {
                    QypWebT4AvailableDatesAssocMember = new DateTime(2013, 02, 03),
                    QypWebT4YearsAssocMember = "2012"
                },
                new QtdYtdParameterT4WebT4Parameter()
                {
                    QypWebT4AvailableDatesAssocMember = new DateTime(2014, 01, 16),
                    QypWebT4YearsAssocMember = "2013"
                },
                new QtdYtdParameterT4WebT4Parameter()
                {
                    QypWebT4AvailableDatesAssocMember = new DateTime(2015, 01, 20),
                    QypWebT4YearsAssocMember = "2014"
                }
            }
        };

        private Collection<TaxForm1098Years> taxForm1098YearsContracts = new Collection<TaxForm1098Years>()
        {
            new TaxForm1098Years() { Recordkey = "1", Tf98yTaxYear = 2015, Tf98yWebEnabled = "Y" },
            new TaxForm1098Years() { Recordkey = "1", Tf98yTaxYear = 2014, Tf98yWebEnabled = "Y" },
            new TaxForm1098Years() { Recordkey = "1", Tf98yTaxYear = 2013, Tf98yWebEnabled = "Y" },
        };

        private TaxFormStatus taxFormStatusContract = new TaxFormStatus()
        {
            Recordkey = "1098T",
            TfsGenDate = new DateTime(2015, 1, 1),
            TfsStatus = "",
            TfsTaxYear = "2015"
        };

        //private CnstRptParms expectedCnstRptParms = new CnstRptParms()
        //{
        //    CnstT2202aPdfParmsEntityAssociation = new List<CnstRptParmsCnstT2202aPdfParms>()
        //    {
        //        new CnstRptParmsCnstT2202aPdfParms()
        //        {
        //            CnstT2202aPdfTaxYearAssocMember = "2010",
        //            CnstT2202aPdfWebFlagAssocMember = "Y"
        //        },
        //        new CnstRptParmsCnstT2202aPdfParms()
        //        {
        //            CnstT2202aPdfTaxYearAssocMember = "2011",
        //            CnstT2202aPdfWebFlagAssocMember = "Y"
        //        },
        //        new CnstRptParmsCnstT2202aPdfParms()
        //        {
        //            CnstT2202aPdfTaxYearAssocMember = "2012",
        //            CnstT2202aPdfWebFlagAssocMember = "Y"
        //        },
        //        new CnstRptParmsCnstT2202aPdfParms()
        //        {
        //            CnstT2202aPdfTaxYearAssocMember = "2013",
        //            CnstT2202aPdfWebFlagAssocMember = "Y"
        //        },
        //        new CnstRptParmsCnstT2202aPdfParms()
        //        {
        //            CnstT2202aPdfTaxYearAssocMember = "2014",
        //            CnstT2202aPdfWebFlagAssocMember = "Y"
        //        }
        //    }

        //};

        [TestInitialize]
        public void Initialize()
        {
            // Initialize Mock framework
            MockInitialize();

            FakeApiSettingsWithGmtColleagueTimeZone = new ApiSettings()
            {
                // use UTC time zone so this test doesn't fail when DST changes.
                ColleagueTimeZone = "GMT Standard Time"
            };

            FakeBackupConfigurationData = new BackupConfiguration();
            FakeBackupConfigurationData.Id = "56c1fb34-9e3e-49a0-b2b0-60751a877855";
            FakeBackupConfigurationData.Namespace = "Ellucian/Colleague Web API/1.18.0.0/dvetk_wstst01_rt";
            FakeBackupConfigurationData.ProductId = "Colleague Web API";
            FakeBackupConfigurationData.ProductVersion = "1.18.0.0";
            FakeBackupConfigurationData.CreatedDateTime = new DateTimeOffset(2017, 1, 1, 13, 10, 10, new TimeSpan(0));

            FakeBackupConfigReadResponseRecord = new BackupConfigRecords()
            {
                ConfigDataId = FakeBackupConfigurationData.Id,
                ProductId = FakeBackupConfigurationData.ProductId,
                ProductVersion = FakeBackupConfigurationData.ProductVersion,
                ConfigNamespace = FakeBackupConfigurationData.Namespace,
                ConfigVersion = FakeBackupConfigurationData.ConfigVersion,
                ConfigData = FakeBackupConfigurationData.ConfigData,
                AddedDate = FakeBackupConfigurationData.CreatedDateTime.Value.DateTime,
                AddedTime = FakeBackupConfigurationData.CreatedDateTime.Value.DateTime
            };

            FakeBackupConfigReadResponse = new ReadBackupConfigDataResponse()
            {
                BackupConfigRecords = new List<BackupConfigRecords>() { FakeBackupConfigReadResponseRecord }
            };

            // Build the test repository
            ConfigRepository = BuildValidRepository();

        }

        [TestCleanup]
        public void Cleanup()
        {
            ConfigRepository = null;
        }

        #endregion

        [TestClass]
        public class ConfigurationRepository_GetDefaultsConfiguration : ConfigurationRepositoryTests
        {
            [TestMethod]
            public void ConfigurationRepository_GetDefaultsConfiguration_Verify()
            {
                var config = this.ConfigRepository.GetDefaultsConfiguration();
                Assert.AreEqual(config.CampusCalendarId, "MAIN");
            }
        }

        [TestClass]
        public class ConfigurationRepository_GetIntegrationConfiguration : ConfigurationRepositoryTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ConfigurationRepository_GetIntegrationConfiguration_NullConfigId()
            {
                var result = await ConfigRepository.GetIntegrationConfiguration(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ConfigurationRepository_GetIntegrationConfiguration_EmptyConfigId()
            {
                var result = await ConfigRepository.GetIntegrationConfiguration(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task ConfigurationRepository_GetIntegrationConfiguration_NoRecord()
            {
                var result = await ConfigRepository.GetIntegrationConfiguration("NORECORD");
            }

            [TestMethod]
            public async Task ConfigurationRepository_GetIntegrationConfiguration_ValidRecord_NoMappings()
            {
                dataReaderMock.Setup(d => d.ReadRecordAsync<CdmIntegration>("CDM.INTEGRATION", "TEST", It.IsAny<bool>())).ReturnsAsync(cdmIntegrationResponses.Where(r => r.Recordkey == "TEST").FirstOrDefault());

                var result = await ConfigRepository.GetIntegrationConfiguration("TEST");
                Assert.AreEqual(cdmIntegrationResponses[0].Recordkey, result.Id);
                Assert.AreEqual(new System.Uri(cdmIntegrationResponses[0].CintServerBaseUrl).Host, result.AmqpMessageServerBaseUrl);
                Assert.AreEqual(new System.Uri(cdmIntegrationResponses[0].CintServerBaseUrl).Scheme == "https", result.AmqpConnectionIsSecure);
                Assert.AreEqual(new System.Uri(cdmIntegrationResponses[0].CintServerBaseUrl).Port, result.AmqpMessageServerPortNumber);
                Assert.AreEqual(cdmIntegrationResponses[0].CintServerUsername, result.AmqpUsername);
                Assert.AreEqual(cdmIntegrationResponses[0].CintServerPassword, result.AmqpPassword);
                Assert.AreEqual(cdmIntegrationResponses[0].CintBusEventExchange, result.BusinessEventExchangeName);
                Assert.AreEqual(cdmIntegrationResponses[0].CintBusEventQueue, result.BusinessEventQueueName);
                Assert.AreEqual(cdmIntegrationResponses[0].CintOutboundExchange, result.OutboundExchangeName);
                Assert.AreEqual(cdmIntegrationResponses[0].CintInboundExchange, result.InboundExchangeName);
                Assert.AreEqual(cdmIntegrationResponses[0].CintInboundQueue, result.InboundExchangeQueueName);
                Assert.AreEqual(cdmIntegrationResponses[0].CintApiUsername, result.ApiUsername);
                Assert.AreEqual(cdmIntegrationResponses[0].CintApiPassword, result.ApiPassword);
                Assert.AreEqual(cdmIntegrationResponses[0].CintApiErp, result.ApiErpName);
                Assert.AreEqual(cdmIntegrationResponses[0].CintServerVirtHost, result.AmqpMessageServerVirtualHost);
                Assert.AreEqual(cdmIntegrationResponses[0].CintServerTimeout, result.AmqpMessageServerConnectionTimeout);
                bool? cintServerAutorecoverFlag = false;
                cintServerAutorecoverFlag = cdmIntegrationResponses[0].CintServerAutorecoverFlag.ToUpper() == "Y";
                Assert.AreEqual(cintServerAutorecoverFlag, result.AutomaticallyRecoverAmqpMessages);
                Assert.AreEqual(cdmIntegrationResponses[0].CintServerHeartbeat, result.AmqpMessageServerHeartbeat);
                bool? cintUseIntegrationHub = false;
                cintUseIntegrationHub = cdmIntegrationResponses[0].CintUseIntegrationHub.ToUpper() == "Y";
                Assert.AreEqual(cintUseIntegrationHub, result.UseIntegrationHub);
                Assert.AreEqual(cdmIntegrationResponses[0].CintHubApiKey, result.ApiKey);
                Assert.AreEqual(cdmIntegrationResponses[0].CintHubTokenUrl, result.TokenUrl);
                Assert.AreEqual(cdmIntegrationResponses[0].CintHubSubscribeUrl, result.SubscribeUrl);
                Assert.AreEqual(cdmIntegrationResponses[0].CintHubPublishUrl, result.PublishUrl);
                Assert.AreEqual(cdmIntegrationResponses[0].CintHubErrorUrl, result.ErrorUrl);
                Assert.AreEqual(cdmIntegrationResponses[0].CintHubMediaType, result.HubMediaType);
                Assert.AreEqual(cdmIntegrationResponses[0].ApiBusEventMapEntityAssociation.Count, result.ResourceBusinessEventMappings.Count);
            }

            [TestMethod]
            public async Task ConfigurationRepository_GetIntegrationConfiguration_ValidRecordWithMappings()
            {
                dataReaderMock.Setup(d => d.ReadRecordAsync<CdmIntegration>("CDM.INTEGRATION", "TEST2", It.IsAny<bool>())).ReturnsAsync(cdmIntegrationResponses.Where(r => r.Recordkey == "TEST2").FirstOrDefault());
                var result = await ConfigRepository.GetIntegrationConfiguration("TEST2");
                Assert.AreEqual(cdmIntegrationResponses[1].Recordkey, result.Id);
                Assert.AreEqual(new System.Uri(cdmIntegrationResponses[1].CintServerBaseUrl).Host, result.AmqpMessageServerBaseUrl);
                Assert.AreEqual(new System.Uri(cdmIntegrationResponses[1].CintServerBaseUrl).Scheme == "https", result.AmqpConnectionIsSecure);
                Assert.AreEqual(new System.Uri(cdmIntegrationResponses[1].CintServerBaseUrl).Port, result.AmqpMessageServerPortNumber);
                Assert.AreEqual(cdmIntegrationResponses[1].CintServerUsername, result.AmqpUsername);
                Assert.AreEqual(cdmIntegrationResponses[1].CintServerPassword, result.AmqpPassword);
                Assert.AreEqual(cdmIntegrationResponses[1].CintBusEventExchange, result.BusinessEventExchangeName);
                Assert.AreEqual(cdmIntegrationResponses[1].CintBusEventQueue, result.BusinessEventQueueName);
                Assert.AreEqual(cdmIntegrationResponses[1].CintOutboundExchange, result.OutboundExchangeName);
                Assert.AreEqual(cdmIntegrationResponses[1].CintInboundExchange, result.InboundExchangeName);
                Assert.AreEqual(cdmIntegrationResponses[1].CintInboundQueue, result.InboundExchangeQueueName);
                Assert.AreEqual(cdmIntegrationResponses[1].CintApiUsername, result.ApiUsername);
                Assert.AreEqual(cdmIntegrationResponses[1].CintApiPassword, result.ApiPassword);
                Assert.AreEqual(cdmIntegrationResponses[1].CintApiErp, result.ApiErpName);
                Assert.AreEqual(cdmIntegrationResponses[1].CintServerVirtHost, result.AmqpMessageServerVirtualHost);
                Assert.AreEqual(cdmIntegrationResponses[1].CintServerTimeout, result.AmqpMessageServerConnectionTimeout);
                bool? cintServerAutorecoverFlag = false;
                cintServerAutorecoverFlag = cdmIntegrationResponses[1].CintServerAutorecoverFlag.ToUpper() == "Y";
                Assert.AreEqual(cintServerAutorecoverFlag, result.AutomaticallyRecoverAmqpMessages);
                Assert.AreEqual(cdmIntegrationResponses[1].CintServerHeartbeat, result.AmqpMessageServerHeartbeat);
                bool? cintUseIntegrationHub = false;
                cintUseIntegrationHub = cdmIntegrationResponses[1].CintUseIntegrationHub.ToUpper() == "Y";
                Assert.AreEqual(cintUseIntegrationHub, result.UseIntegrationHub);
                Assert.AreEqual(cdmIntegrationResponses[1].CintHubApiKey, result.ApiKey);
                Assert.AreEqual(cdmIntegrationResponses[1].CintHubTokenUrl, result.TokenUrl);
                Assert.AreEqual(cdmIntegrationResponses[1].CintHubSubscribeUrl, result.SubscribeUrl);
                Assert.AreEqual(cdmIntegrationResponses[1].CintHubPublishUrl, result.PublishUrl);
                Assert.AreEqual(cdmIntegrationResponses[1].CintHubErrorUrl, result.ErrorUrl);
                Assert.AreEqual(cdmIntegrationResponses[1].CintHubMediaType, result.HubMediaType);

                Assert.AreEqual(cdmIntegrationResponses[1].ApiBusEventMapEntityAssociation.Count, result.ResourceBusinessEventMappings.Count);
                for (int i = 0; i < result.ResourceBusinessEventMappings.Count; i++)
                {
                    Assert.AreEqual(cdmIntegrationResponses[1].ApiBusEventMapEntityAssociation[i].CintApiResourceAssocMember, result.ResourceBusinessEventMappings[i].ResourceName);
                    Assert.AreEqual(cdmIntegrationResponses[1].ApiBusEventMapEntityAssociation[i].CintApiRsrcSchemaSemVerAssocMember, result.ResourceBusinessEventMappings[i].ResourceVersion);
                    Assert.AreEqual(cdmIntegrationResponses[1].ApiBusEventMapEntityAssociation[i].CintApiPathAssocMember, result.ResourceBusinessEventMappings[i].PathSegment);
                    Assert.AreEqual(cdmIntegrationResponses[1].ApiBusEventMapEntityAssociation[i].CintApiBusEventsAssocMember, result.ResourceBusinessEventMappings[i].BusinessEvent);
                }

                Assert.AreEqual(cdmIntegrationResponses[1].CintInboundRoutingKeys.Count, result.InboundExchangeRoutingKeys.Count);
                for (int j = 0; j < result.InboundExchangeRoutingKeys.Count; j++)
                {
                    Assert.AreEqual(cdmIntegrationResponses[1].CintInboundRoutingKeys[j], result.InboundExchangeRoutingKeys[j]);
                }

                Assert.AreEqual(cdmIntegrationResponses[1].CintBusEventRoutingKeys.Count, result.BusinessEventRoutingKeys.Count);
                for (int k = 0; k < result.BusinessEventRoutingKeys.Count; k++)
                {
                    Assert.AreEqual(cdmIntegrationResponses[1].CintBusEventRoutingKeys[k], result.BusinessEventRoutingKeys[k]);
                }
            }
        }

        [TestClass]
        public class ConfigurationRepository_GetExternalMapping : ConfigurationRepositoryTests
        {
            [TestInitialize]
            public void GetExternalMapping_Initialize()
            {
                var corruptRecord = new ElfTranslateTables() { Recordkey = "LDM.TEST" };
                dataReaderMock.Setup<ElfTranslateTables>(
                    reader => reader.ReadRecord<ElfTranslateTables>(It.IsAny<string>(), It.IsAny<bool>()))
                    .Returns<string, bool>((id, replaceVm) =>
                    {
                        if (id == corruptRecord.Recordkey)
                        {
                            return corruptRecord;
                        }
                        else
                        {
                            var table = elfTranslateTables.FirstOrDefault(x => x.Recordkey == id);
                            return table;
                        }
                    }
                );
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ConfigurationRepository_GetExternalMapping_NullId()
            {
                var map = this.ConfigRepository.GetExternalMapping(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ConfigurationRepository_GetExternalMapping_EmptyId()
            {
                var map = this.ConfigRepository.GetExternalMapping(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public void ConfigurationRepository_GetExternalMapping_InvalidId()
            {
                var map = this.ConfigRepository.GetExternalMapping("INVALID");
            }

            [TestMethod]
            public void ConfigurationRepository_GetExternalMapping_Verify()
            {
                for (int i = 0; i < elfTranslateTables.Count; i++)
                {
                    var map = this.ConfigRepository.GetExternalMapping(elfTranslateTables[i].Recordkey);
                    Assert.AreEqual(elfTranslateTables[i].Recordkey, map.Code);
                    Assert.AreEqual(elfTranslateTables[i].ElftDesc, map.Description);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ConfigurationException))]
            public void ConfigurationRepository_GetExternalMapping_CorruptRecord()
            {
                var map = this.ConfigRepository.GetExternalMapping("LDM.TEST");
            }
        }

        #region GetTaxFormConsentConfigurationAsync
        [TestClass, System.Runtime.InteropServices.GuidAttribute("23894E0F-C5FD-4566-A92B-776992289D6A")]
        public class ConfigurationRepository_GetTaxFormConfiguration : ConfigurationRepositoryTests
        {
            #region W-2
            [TestMethod]
            public async Task GetTaxFormConfigurationAsync_Success_W2()
            {
                var actualConfiguration = await this.ConfigRepository.GetTaxFormConsentConfigurationAsync(TaxForms.FormW2);

                // Check that the W-2 consent paragraphs are the same
                Assert.AreEqual(expectedHrWebDefaults.HrwebW2oConText, actualConfiguration.ConsentParagraphs.ConsentText);
                Assert.AreEqual(expectedHrWebDefaults.HrwebW2oWhldText, actualConfiguration.ConsentParagraphs.ConsentWithheldText);

                var actualAvailabilityConfiguration = await this.ConfigRepository.GetTaxFormAvailabilityConfigurationAsync(TaxForms.FormW2);
                // Make sure the domain entity set has the same number of year/date entries as the data contract set for W-2.
                Assert.AreEqual(expectedQtdYtdParameterW2.WebW2ParametersEntityAssociation.Count, actualAvailabilityConfiguration.Availabilities.Count);

                // Make sure each data contract entry is reflected in the domain entity set for W-2.
                foreach (var dataContract in expectedQtdYtdParameterW2.WebW2ParametersEntityAssociation)
                {
                    // Get the domain entity with matching data
                    var domainEntity = actualAvailabilityConfiguration.Availabilities.Where(x =>
                        x.TaxYear == dataContract.QypWebW2YearsAssocMember
                        && x.Available == DateTime.Compare(dataContract.QypWebW2AvailableDatesAssocMember.Value, DateTime.Now) <= 0).ToList();

                    // Make sure the domainEntity list contains only one object.
                    Assert.AreEqual(1, domainEntity.Count);
                }
            }

            [TestMethod]
            public async Task GetTaxFormConfigurationAsync_W2_DataReaderReturnsNull()
            {
                dataReaderMock.Setup<Task<HrwebDefaults>>(datareader => datareader.ReadRecordAsync<HrwebDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(() =>
                {
                    HrwebDefaults defaults = new HrwebDefaults();
                    defaults = null;
                    return Task.FromResult(defaults);
                });

                dataReaderMock.Setup<Task<QtdYtdParameterW2>>(datareader => datareader.ReadRecordAsync<QtdYtdParameterW2>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(() =>
                {
                    QtdYtdParameterW2 parameters = new QtdYtdParameterW2();
                    parameters = null;
                    return Task.FromResult(parameters);
                });

                var actualConfiguration = await this.ConfigRepository.GetTaxFormConsentConfigurationAsync(TaxForms.FormW2);

                // Check W-2
                Assert.IsTrue(actualConfiguration is TaxFormConfiguration);
                Assert.AreEqual(null, actualConfiguration.ConsentParagraphs.ConsentText);
                Assert.AreEqual(null, actualConfiguration.ConsentParagraphs.ConsentWithheldText);
                Assert.AreEqual(0, actualConfiguration.Availabilities.Count);
            }

            [TestMethod]
            public async Task GetTaxFormConfigurationAsync_W2_NullTaxYear()
            {
                expectedQtdYtdParameterW2.QypWebW2Years[0] = null;

                var actualAvailabilityConfiguration = await this.ConfigRepository.GetTaxFormAvailabilityConfigurationAsync(TaxForms.FormW2);

                // Make sure the domain entity set has the same number of year/date entries as the data contract set for W-2
                Assert.AreEqual(expectedQtdYtdParameterW2.WebW2ParametersEntityAssociation.Count - 1, actualAvailabilityConfiguration.Availabilities.Count);

                // Make sure each domain entity returned is reflected in the original data contract set for W-2
                foreach (var domainEntity in actualAvailabilityConfiguration.Availabilities)
                {
                    // Get the domain entity with matching data
                    var dataContract = expectedQtdYtdParameterW2.WebW2ParametersEntityAssociation.Where(x =>
                        x.QypWebW2YearsAssocMember == domainEntity.TaxYear
                        && (DateTime.Compare(x.QypWebW2AvailableDatesAssocMember.Value, DateTime.Now) <= 0) == domainEntity.Available).ToList();

                    // Make sure the domainEntity list contains only one object
                    Assert.AreEqual(1, dataContract.Count);
                }
            }

            [TestMethod]
            public async Task GetTaxFormConfigurationAsync_W2_EmptyTaxYear()
            {
                expectedQtdYtdParameterW2.QypWebW2Years[0] = string.Empty;

                var actualAvailabilityConfiguration = await this.ConfigRepository.GetTaxFormAvailabilityConfigurationAsync(TaxForms.FormW2);

                // Make sure the domain entity set has the same number of year/date entries as the data contract set for W-2
                Assert.AreEqual(expectedQtdYtdParameterW2.WebW2ParametersEntityAssociation.Count - 1, actualAvailabilityConfiguration.Availabilities.Count);

                // Make sure each domain entity returned is reflected in the original data contract set for W-2
                foreach (var domainEntity in actualAvailabilityConfiguration.Availabilities)
                {
                    // Get the domain entity with matching data
                    var dataContract = expectedQtdYtdParameterW2.WebW2ParametersEntityAssociation.Where(x =>
                        x.QypWebW2YearsAssocMember == domainEntity.TaxYear
                        && (DateTime.Compare(x.QypWebW2AvailableDatesAssocMember.Value, DateTime.Now) <= 0) == domainEntity.Available).ToList();

                    // Make sure the domainEntity list contains only one object
                    Assert.AreEqual(1, dataContract.Count);
                }
            }

            [TestMethod]
            public async Task GetTaxFormConfigurationAsync_NullAvailableDate_W2()
            {
                expectedQtdYtdParameterW2.QypWebW2AvailableDates[0] = null;

                var actualAvailabilityConfiguration = await this.ConfigRepository.GetTaxFormAvailabilityConfigurationAsync(TaxForms.FormW2);

                // Make sure the domain entity set has the same number of year/date entries as the data contract set
                Assert.AreEqual(expectedQtdYtdParameterW2.WebW2ParametersEntityAssociation.Count - 1, actualAvailabilityConfiguration.Availabilities.Count);

                // Make sure each domain entity returned is reflected in the original data contract set.
                foreach (var domainEntity in actualAvailabilityConfiguration.Availabilities)
                {
                    // Get the domain entity with matching data
                    var dataContract = expectedQtdYtdParameterW2.WebW2ParametersEntityAssociation.Where(x =>
                        x.QypWebW2YearsAssocMember == domainEntity.TaxYear
                        && (DateTime.Compare(x.QypWebW2AvailableDatesAssocMember.Value, DateTime.Now) <= 0) == domainEntity.Available).ToList();

                    // Make sure the domainEntity list contains only one object.
                    Assert.AreEqual(1, dataContract.Count);
                }
            }

            [TestMethod]
            public async Task GetTaxFormConfigurationAsync_EmptyAssociation_W2()
            {
                dataReaderMock.Setup<Task<QtdYtdParameterW2>>(
                    dataReader => dataReader.ReadRecordAsync<QtdYtdParameterW2>(It.IsAny<string>(), It.IsAny<string>(), true))
                    .Returns(() =>
                    {
                        // Build the association for W2 data
                        expectedQtdYtdParameterW2.buildAssociations();

                        // Make the W-2 association null
                        expectedQtdYtdParameterW2.WebW2ParametersEntityAssociation = null;

                        return Task.FromResult(expectedQtdYtdParameterW2);
                    });

                var actualConfiguration = await this.ConfigRepository.GetTaxFormConsentConfigurationAsync(TaxForms.FormW2);

                // The availability list should be empty
                Assert.AreEqual(0, actualConfiguration.Availabilities.Count);
            }
            #endregion

            #region 1095-C
            [TestMethod]
            public async Task GetTaxFormConfigurationAsync_Success_1095C()
            {
                var actualConfiguration = await this.ConfigRepository.GetTaxFormConsentConfigurationAsync(TaxForms.Form1095C);

                // Check that the 1095-C consent paragraphs are the same
                Assert.AreEqual(expectedHrWebDefaults.Hrweb1095cConText, actualConfiguration.ConsentParagraphs.ConsentText);
                Assert.AreEqual(expectedHrWebDefaults.Hrweb1095cWhldText, actualConfiguration.ConsentParagraphs.ConsentWithheldText);

                var actualAvailabilityConfiguration = await this.ConfigRepository.GetTaxFormAvailabilityConfigurationAsync(TaxForms.Form1095C);
                // Make sure the domain entity set has the same number of year/date entries as the data contract set for 1095-C.
                Assert.AreEqual(expectedQtdYtdParameter1095C.Qyp1095cParametersEntityAssociation.Count, actualAvailabilityConfiguration.Availabilities.Count);

                foreach (var dataContract in expectedQtdYtdParameter1095C.Qyp1095cParametersEntityAssociation)
                {
                    // Get the domain entity with matching data
                    var domainEntity = actualAvailabilityConfiguration.Availabilities.Where(x =>
                        x.TaxYear == dataContract.QypWeb1095cYearsAssocMember
                        && x.Available == DateTime.Compare(dataContract.QypWeb1095cAvailDatesAssocMember.Value, DateTime.Now) <= 0).ToList();

                    // Make sure the domainEntity list contains only one object.
                    Assert.AreEqual(1, domainEntity.Count);
                }
            }

            [TestMethod]
            public async Task GetTaxFormConfigurationAsync_1095C_DataReaderReturnsNull()
            {
                dataReaderMock.Setup<Task<HrwebDefaults>>(datareader => datareader.ReadRecordAsync<HrwebDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(() =>
                {
                    HrwebDefaults defaults = new HrwebDefaults();
                    defaults = null;
                    return Task.FromResult(defaults);
                });

                dataReaderMock.Setup<Task<QtdYtdParameter1095C>>(datareader => datareader.ReadRecordAsync<QtdYtdParameter1095C>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(() =>
                {
                    QtdYtdParameter1095C parameters = new QtdYtdParameter1095C();
                    parameters = null;
                    return Task.FromResult(parameters);
                });

                var actualConfiguration = await this.ConfigRepository.GetTaxFormConsentConfigurationAsync(TaxForms.Form1095C);

                // Check 1095-C
                Assert.IsTrue(actualConfiguration is TaxFormConfiguration);
                Assert.AreEqual(null, actualConfiguration.ConsentParagraphs.ConsentText);
                Assert.AreEqual(null, actualConfiguration.ConsentParagraphs.ConsentWithheldText);
                Assert.AreEqual(0, actualConfiguration.Availabilities.Count);
            }

            [TestMethod]
            public async Task GetTaxFormConfigurationAsync_1095C_NullTaxYear()
            {
                expectedQtdYtdParameter1095C.QypWeb1095cYears[0] = null;

                var actualAvailabilityConfiguration = await this.ConfigRepository.GetTaxFormAvailabilityConfigurationAsync(TaxForms.Form1095C);

                // Make sure the domain entity set has the same number of year/date entries as the data contract set for 1095-C
                Assert.AreEqual(expectedQtdYtdParameter1095C.Qyp1095cParametersEntityAssociation.Count - 1, actualAvailabilityConfiguration.Availabilities.Count);

                // Make sure each domain entity returned is reflected in the original data contract set for 1095-C
                foreach (var domainEntity in actualAvailabilityConfiguration.Availabilities)
                {
                    // Get the domain entity with matching data
                    var dataContract = expectedQtdYtdParameter1095C.Qyp1095cParametersEntityAssociation.Where(x =>
                        x.QypWeb1095cYearsAssocMember == domainEntity.TaxYear
                        && (DateTime.Compare(x.QypWeb1095cAvailDatesAssocMember.Value, DateTime.Now) <= 0) == domainEntity.Available).ToList();

                    // Make sure the domainEntity list contains only one object
                    Assert.AreEqual(1, dataContract.Count);
                }
            }

            [TestMethod]
            public async Task GetTaxFormConfigurationAsync_EmptyTaxYear_1095C()
            {
                expectedQtdYtdParameter1095C.QypWeb1095cYears[0] = string.Empty;

                var actualAvailabilityConfiguration = await this.ConfigRepository.GetTaxFormAvailabilityConfigurationAsync(TaxForms.Form1095C);

                // Make sure the domain entity set has the same number of year/date entries as the data contract set for 1095-C
                Assert.AreEqual(expectedQtdYtdParameter1095C.Qyp1095cParametersEntityAssociation.Count - 1, actualAvailabilityConfiguration.Availabilities.Count);

                // Make sure each domain entity returned is reflected in the original data contract set for 1095-C
                foreach (var domainEntity in actualAvailabilityConfiguration.Availabilities)
                {
                    // Get the domain entity with matching data
                    var dataContract = expectedQtdYtdParameter1095C.Qyp1095cParametersEntityAssociation.Where(x =>
                        x.QypWeb1095cYearsAssocMember == domainEntity.TaxYear
                        && (DateTime.Compare(x.QypWeb1095cAvailDatesAssocMember.Value, DateTime.Now) <= 0) == domainEntity.Available).ToList();

                    // Make sure the domainEntity list contains only one object
                    Assert.AreEqual(1, dataContract.Count);
                }
            }

            [TestMethod]
            public async Task GetTaxFormConfigurationAsync_NullAvailableDate_1095C()
            {
                expectedQtdYtdParameter1095C.QypWeb1095cAvailDates[0] = null;

                var actualAvailabilityConfiguration = await this.ConfigRepository.GetTaxFormAvailabilityConfigurationAsync(TaxForms.Form1095C);

                // Make sure the domain entity set has the same number of year/date entries as the data contract set for 1095-C
                Assert.AreEqual(expectedQtdYtdParameter1095C.Qyp1095cParametersEntityAssociation.Count - 1, actualAvailabilityConfiguration.Availabilities.Count);

                // Make sure each domain entity returned is reflected in the original data contract set for 1095-C
                foreach (var domainEntity in actualAvailabilityConfiguration.Availabilities)
                {
                    // Get the domain entity with matching data
                    var dataContract = expectedQtdYtdParameter1095C.Qyp1095cParametersEntityAssociation.Where(x =>
                        x.QypWeb1095cYearsAssocMember == domainEntity.TaxYear
                        && (DateTime.Compare(x.QypWeb1095cAvailDatesAssocMember.Value, DateTime.Now) <= 0) == domainEntity.Available).ToList();

                    // Make sure the domainEntity list contains only one object
                    Assert.AreEqual(1, dataContract.Count);
                }
            }

            [TestMethod]
            public async Task GetTaxFormConfigurationAsync_EmptyAssociation_1095C()
            {
                dataReaderMock.Setup<Task<QtdYtdParameter1095C>>(
                    dataReader => dataReader.ReadRecordAsync<QtdYtdParameter1095C>(It.IsAny<string>(), It.IsAny<string>(), true))
                    .Returns(() =>
                    {
                        // Build the association for 1095-C data
                        expectedQtdYtdParameter1095C.buildAssociations();

                        // Make the 1095-C association null
                        expectedQtdYtdParameter1095C.Qyp1095cParametersEntityAssociation = null;

                        return Task.FromResult(expectedQtdYtdParameter1095C);
                    });

                var actualConfiguration = await this.ConfigRepository.GetTaxFormConsentConfigurationAsync(TaxForms.Form1095C);

                // The availability list should be empty.
                Assert.AreEqual(0, actualConfiguration.Availabilities.Count);
            }
            #endregion

            #region 1098
            [TestMethod]
            public async Task GetTaxFormConfigurationAsync_Success_1098()
            {
                var actualConfiguration = await this.ConfigRepository.GetTaxFormConsentConfigurationAsync(TaxForms.Form1098);

                // Check that the 1095-C consent paragraphs are the same
                Assert.AreEqual(parm1098DataContract.P1098ConsentText, actualConfiguration.ConsentParagraphs.ConsentText);
                Assert.AreEqual(parm1098DataContract.P1098WhldConsentText, actualConfiguration.ConsentParagraphs.ConsentWithheldText);
            }

            [TestMethod]
            public async Task GetTaxFormConfigurationAsync_1098_DataReaderReturnsNull()
            {
                dataReaderMock.Setup<Task<Parm1098>>(datareader => datareader.ReadRecordAsync<Parm1098>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(() =>
                {
                    Parm1098 defaults = new Parm1098();
                    defaults = null;
                    return Task.FromResult(defaults);
                });

                var actualConfiguration = await this.ConfigRepository.GetTaxFormConsentConfigurationAsync(TaxForms.Form1098);

                // Check 1098
                Assert.IsTrue(actualConfiguration is TaxFormConfiguration);
                Assert.AreEqual(null, actualConfiguration.ConsentParagraphs.ConsentText);
                Assert.AreEqual(null, actualConfiguration.ConsentParagraphs.ConsentWithheldText);
                Assert.AreEqual(0, actualConfiguration.Availabilities.Count);
            }
            #endregion

            #region T4
            [TestMethod]
            public async Task GetTaxFormConfigurationAsync_Success_T4()
            {
                var actualConfiguration = await this.ConfigRepository.GetTaxFormConsentConfigurationAsync(TaxForms.FormT4);

                // Check that the 1095-C consent paragraphs are the same
                Assert.AreEqual(parmT4DataContract.Pt4ConText, actualConfiguration.ConsentParagraphs.ConsentText);
                Assert.AreEqual(parmT4DataContract.Pt4WhldText, actualConfiguration.ConsentParagraphs.ConsentWithheldText);
            }

            [TestMethod]
            public async Task GetTaxFormConfigurationAsync_T4_DataReaderReturnsNull()
            {
                parmT4DataContract = null;
                var actualConfiguration = await this.ConfigRepository.GetTaxFormConsentConfigurationAsync(TaxForms.FormT4);

                Assert.IsTrue(actualConfiguration is TaxFormConfiguration);
                Assert.AreEqual(null, actualConfiguration.ConsentParagraphs.ConsentText);
                Assert.AreEqual(null, actualConfiguration.ConsentParagraphs.ConsentWithheldText);
            }
            #endregion

            #region T4A
            [TestMethod]
            public async Task GetTaxFormConfigurationAsync_Success_T4A()
            {
                var actualConfiguration = await this.ConfigRepository.GetTaxFormConsentConfigurationAsync(TaxForms.FormT4A);

                // Check that the 1095-C consent paragraphs are the same
                Assert.AreEqual(parmT4ADataContract.Pt4aConText, actualConfiguration.ConsentParagraphs.ConsentText);
                Assert.AreEqual(parmT4ADataContract.Pt4aWhldText, actualConfiguration.ConsentParagraphs.ConsentWithheldText);
            }

            [TestMethod]
            public async Task GetTaxFormConfigurationAsync_T4A_DataReaderReturnsNull()
            {
                parmT4ADataContract = null;
                var actualConfiguration = await this.ConfigRepository.GetTaxFormConsentConfigurationAsync(TaxForms.FormT4A);

                Assert.IsTrue(actualConfiguration is TaxFormConfiguration);
                Assert.AreEqual(null, actualConfiguration.ConsentParagraphs.ConsentText);
                Assert.AreEqual(null, actualConfiguration.ConsentParagraphs.ConsentWithheldText);
            }
            #endregion

            #region T2202A
            [TestMethod]
            public async Task GetTaxFormConfigurationAsync_Success_T2202A()
            {
                var actualConfiguration = await this.ConfigRepository.GetTaxFormConsentConfigurationAsync(TaxForms.FormT2202A);

                // Check that the 1095-C consent paragraphs are the same
                Assert.AreEqual(cnstRptParmsContract.CnstConsentText, actualConfiguration.ConsentParagraphs.ConsentText);
                Assert.AreEqual(cnstRptParmsContract.CnstWhldConsentText, actualConfiguration.ConsentParagraphs.ConsentWithheldText);
            }

            [TestMethod]
            public async Task GetTaxFormConfigurationAsync_T2202A_DataReaderReturnsNull()
            {
                cnstRptParmsContract = null;
                var actualConfiguration = await this.ConfigRepository.GetTaxFormConsentConfigurationAsync(TaxForms.FormT2202A);

                Assert.IsTrue(actualConfiguration is TaxFormConfiguration);
                Assert.AreEqual(null, actualConfiguration.ConsentParagraphs.ConsentText);
                Assert.AreEqual(null, actualConfiguration.ConsentParagraphs.ConsentWithheldText);
            }
            #endregion
        }
        #endregion

        #region GetTaxFormAvailabilityConfigurationAsync 1098
        [TestMethod]
        public async Task GetTaxFormAvailabilityConfigurationAsync_AllYearsAvailable()
        {
            var actualConfiguration = await ConfigRepository.GetTaxFormAvailabilityConfigurationAsync(TaxForms.Form1098T);

            Assert.AreEqual(TaxForms.Form1098T, actualConfiguration.TaxFormId);
            foreach (var dataContract in taxForm1098YearsContracts)
            {
                var actualAvailabilityYears = actualConfiguration.Availabilities.Where(x =>
                    x.TaxYear == dataContract.Tf98yTaxYear.ToString()
                    && x.Available).ToList();
                Assert.AreEqual(1, actualAvailabilityYears.Count);
            }
        }

        [TestMethod]
        public async Task GetTaxFormAvailabilityConfigurationAsync_NullParm1098Contract()
        {
            parm1098DataContract = null;
            var actualConfiguration = await ConfigRepository.GetTaxFormAvailabilityConfigurationAsync(TaxForms.Form1098T);

            Assert.AreEqual(0, actualConfiguration.Availabilities.Count);
        }

        [TestMethod]
        public async Task GetTaxFormAvailabilityConfigurationAsync_NullTaxFormInParm1098()
        {
            parm1098DataContract.P1098TTaxForm = null;
            var actualConfiguration = await ConfigRepository.GetTaxFormAvailabilityConfigurationAsync(TaxForms.Form1098T);

            Assert.AreEqual(0, actualConfiguration.Availabilities.Count);
        }

        [TestMethod]
        public async Task GetTaxFormAvailabilityConfigurationAsync_EmptyTaxFormInParm1098()
        {
            parm1098DataContract.P1098TTaxForm = "";
            var actualConfiguration = await ConfigRepository.GetTaxFormAvailabilityConfigurationAsync(TaxForms.Form1098T);

            Assert.AreEqual(0, actualConfiguration.Availabilities.Count);
        }

        [TestMethod]
        public async Task GetTaxFormAvailabilityConfigurationAsync_TaxFormStatusDataReadReturnsNull()
        {
            taxFormStatusContract = null;
            var actualConfiguration = await ConfigRepository.GetTaxFormAvailabilityConfigurationAsync(TaxForms.Form1098T);

            Assert.AreEqual(0, actualConfiguration.Availabilities.Count);
        }

        [TestMethod]
        public async Task GetTaxFormAvailabilityConfigurationAsync_TaxFormStatusTfsGenDateNull()
        {
            taxFormStatusContract.TfsGenDate = null;
            var actualConfiguration = await ConfigRepository.GetTaxFormAvailabilityConfigurationAsync(TaxForms.Form1098T);

            Assert.AreEqual(2, actualConfiguration.Availabilities.Where(a => a.Available).Count());
        }

        [TestMethod]
        public async Task GetTaxFormAvailabilityConfigurationAsync_TaxFormStatusTfsStatusGEN()
        {
            taxFormStatusContract.TfsStatus = "GEN";
            var actualConfiguration = await ConfigRepository.GetTaxFormAvailabilityConfigurationAsync(TaxForms.Form1098T);

            Assert.AreEqual(taxForm1098YearsContracts.Count - 1, actualConfiguration.Availabilities.Where(a => a.Available).Count());
        }

        [TestMethod]
        public async Task GetTaxFormAvailabilityConfigurationAsync_TaxFormStatusTfsStatusMOD()
        {
            taxFormStatusContract.TfsStatus = "MOD";
            var actualConfiguration = await ConfigRepository.GetTaxFormAvailabilityConfigurationAsync(TaxForms.Form1098T);

            Assert.AreEqual(taxForm1098YearsContracts.Count - 1, actualConfiguration.Availabilities.Where(a => a.Available).Count());
        }

        [TestMethod]
        public async Task GetTaxFormAvailabilityConfigurationAsync_TaxFormStatusTfsStatusUNF()
        {
            taxFormStatusContract.TfsStatus = "UNF";
            var actualConfiguration = await ConfigRepository.GetTaxFormAvailabilityConfigurationAsync(TaxForms.Form1098T);

            Assert.AreEqual(taxForm1098YearsContracts.Count - 1, actualConfiguration.Availabilities.Where(a => a.Available).Count());
        }

        [TestMethod]
        public async Task GetTaxFormAvailabilityConfigurationAsync_TaxForm1098YearsTf98yTaxYearNull()
        {
            foreach (var year in taxForm1098YearsContracts)
            {
                year.Tf98yTaxYear = null;
            }
            var actualConfiguration = await ConfigRepository.GetTaxFormAvailabilityConfigurationAsync(TaxForms.Form1098T);

            Assert.AreEqual(0, actualConfiguration.Availabilities.Count);
        }

        [TestMethod]
        public async Task GetTaxFormAvailabilityConfigurationAsync_TaxForm1098YearsTf98yWebEnabledNull()
        {
            foreach (var year in taxForm1098YearsContracts)
            {
                year.Tf98yWebEnabled = null;
            }
            var actualConfiguration = await ConfigRepository.GetTaxFormAvailabilityConfigurationAsync(TaxForms.Form1098T);

            Assert.AreEqual(0, actualConfiguration.Availabilities.Count);
        }

        [TestMethod]
        public async Task GetTaxFormAvailabilityConfigurationAsync_TaxForm1098YearsTf98yWebEnabledNo()
        {
            foreach (var year in taxForm1098YearsContracts)
            {
                year.Tf98yWebEnabled = "N";
            }
            var actualConfiguration = await ConfigRepository.GetTaxFormAvailabilityConfigurationAsync(TaxForms.Form1098T);

            Assert.AreEqual(3, actualConfiguration.Availabilities.Count);
        }

        [TestMethod]
        public async Task GetTaxFormAvailabilityConfigurationAsync_1098E_AllYearsAvailable()
        {
            var actualConfiguration = await ConfigRepository.GetTaxFormAvailabilityConfigurationAsync(TaxForms.Form1098E);

            Assert.AreEqual(TaxForms.Form1098E, actualConfiguration.TaxFormId);
            foreach (var dataContract in taxForm1098YearsContracts)
            {
                var actualAvailabilityYears = actualConfiguration.Availabilities.Where(x =>
                    x.TaxYear == dataContract.Tf98yTaxYear.ToString()
                    && x.Available).ToList();
                Assert.AreEqual(1, actualAvailabilityYears.Count);
            }
        }

        [TestMethod]
        public async Task GetTaxFormAvailabilityConfigurationAsync_1098E_NullTaxFormInParm1098()
        {
            parm1098DataContract.P1098TTaxForm = null;
            parm1098DataContract.P1098ETaxForm = null;
            var actualConfiguration = await ConfigRepository.GetTaxFormAvailabilityConfigurationAsync(TaxForms.Form1098E);

            Assert.AreEqual(0, actualConfiguration.Availabilities.Count);
        }

        [TestMethod]
        public async Task GetTaxFormAvailabilityConfigurationAsync_1098E_EmptyTaxFormInParm1098()
        {
            parm1098DataContract.P1098TTaxForm = "";
            parm1098DataContract.P1098ETaxForm = "";
            var actualConfiguration = await ConfigRepository.GetTaxFormAvailabilityConfigurationAsync(TaxForms.Form1098E);

            Assert.AreEqual(0, actualConfiguration.Availabilities.Count);
        }
        #endregion

        #region GetTaxFormAvailabilityConfigurationAsync T2202A
        [TestMethod]
        public async Task GetTaxFormAvailabilityConfigurationAsync_AllT2202aYearsAvailable()
        {
            var actualConfiguration = await ConfigRepository.GetTaxFormAvailabilityConfigurationAsync(TaxForms.FormT2202A);

            Assert.AreEqual(TaxForms.FormT2202A, actualConfiguration.TaxFormId);
            foreach (var dataContract in cnstRptParmsContract.CnstT2202aPdfParmsEntityAssociation)
            {
                var actualAvailabilityYears = actualConfiguration.Availabilities.Where(x =>
                    x.TaxYear == dataContract.CnstT2202aPdfTaxYearAssocMember.ToString()
                    && x.Available).ToList();
                Assert.AreEqual(1, actualAvailabilityYears.Count);
            }
        }

        [TestMethod]
        public async Task GetTaxFormAvailabilityConfigurationAsync_NullCnstRptParmsContract()
        {
            cnstRptParmsContract = null;
            var actualConfiguration = await ConfigRepository.GetTaxFormAvailabilityConfigurationAsync(TaxForms.FormT2202A);

            Assert.AreEqual(0, actualConfiguration.Availabilities.Count);
        }

        [TestMethod]
        public async Task GetTaxFormAvailabilityConfigurationAsync_CnstRptParmsTaxYearNull()
        {
            foreach (var year in cnstRptParmsContract.CnstT2202aPdfParmsEntityAssociation)
            {
                year.CnstT2202aPdfTaxYearAssocMember = null;
            }
            var actualConfiguration = await ConfigRepository.GetTaxFormAvailabilityConfigurationAsync(TaxForms.FormT2202A);

            Assert.AreEqual(0, actualConfiguration.Availabilities.Count);
        }

        [TestMethod]
        public async Task GetTaxFormAvailabilityConfigurationAsync_CnstRptParmsWebEnabledNull()
        {
            foreach (var year in cnstRptParmsContract.CnstT2202aPdfParmsEntityAssociation)
            {
                year.CnstT2202aPdfWebFlagAssocMember = null;
            }
            var actualConfiguration = await ConfigRepository.GetTaxFormAvailabilityConfigurationAsync(TaxForms.FormT2202A);

            Assert.AreEqual(0, actualConfiguration.Availabilities.Count);
        }
        #endregion

        #region GetTaxFormAvailabilityConfigurationAsync T4

        [TestMethod]
        public async Task GetTaxFormAvailabilityConfigurationAsync_T4_AllYearsAvailable()
        {
            var actualConfiguration = await ConfigRepository.GetTaxFormAvailabilityConfigurationAsync(TaxForms.FormT4);

            Assert.AreEqual(TaxForms.FormT4, actualConfiguration.TaxFormId);
            foreach (var dataContract in expectedQtdYtdParameterT4.WebT4ParameterEntityAssociation)
            {
                var actualAvailabilityYears = actualConfiguration.Availabilities.Where(x =>
                    x.TaxYear == dataContract.QypWebT4YearsAssocMember
                    && x.Available).ToList();
                Assert.AreEqual(1, actualAvailabilityYears.Count);
            }
        }

        [TestMethod]
        public async Task GetTaxFormAvailabilityConfigurationAsync_T4_NullQtdYtdParameterT4()
        {
            expectedQtdYtdParameterT4 = null;
            var actualConfiguration = await ConfigRepository.GetTaxFormAvailabilityConfigurationAsync(TaxForms.FormT4);

            Assert.AreEqual(0, actualConfiguration.Availabilities.Count);
        }

        [TestMethod]
        public async Task GetTaxFormAvailabilityConfigurationAsync_T4_TaxYearNull()
        {
            foreach (var year in expectedQtdYtdParameterT4.WebT4ParameterEntityAssociation)
            {
                year.QypWebT4YearsAssocMember = null;
            }
            var actualConfiguration = await ConfigRepository.GetTaxFormAvailabilityConfigurationAsync(TaxForms.FormT4);

            Assert.AreEqual(0, actualConfiguration.Availabilities.Count);
        }

        [TestMethod]
        public async Task GetTaxFormAvailabilityConfigurationAsync_T4_AvailableDateNull()
        {
            foreach (var year in expectedQtdYtdParameterT4.WebT4ParameterEntityAssociation)
            {
                year.QypWebT4AvailableDatesAssocMember = null;
            }
            var actualConfiguration = await ConfigRepository.GetTaxFormAvailabilityConfigurationAsync(TaxForms.FormT4);

            Assert.AreEqual(0, actualConfiguration.Availabilities.Count);
        }

        #endregion

        #region Configuration for UserProfile
        [TestClass]
        public class ConfigurationRepository_GetUserProfileConfigurationAsync : ConfigurationRepositoryTests
        {

            [TestMethod]
            public async Task GetsUserProfileConfiguration_WithViewableAndUpdatableLists()
            {
                var configuration = await ConfigRepository.GetUserProfileConfigurationAsync();
                Assert.AreEqual(corewebDefaults.CorewebUserProfileText, configuration.Text);
                foreach (var expectedAddressType in corewebDefaults.CorewebAddressViewTypes)
                {
                    Assert.IsTrue(configuration.ViewableAddressTypes.Contains(expectedAddressType));
                }
                foreach (var expectedPhoneType in corewebDefaults.CorewebPhoneViewTypes)
                {
                    Assert.IsTrue(configuration.ViewablePhoneTypes.Contains(expectedPhoneType));
                }
                foreach (var expectedEmailType in corewebDefaults.CorewebEmailViewTypes)
                {
                    Assert.IsTrue(configuration.ViewableEmailTypes.Contains(expectedEmailType));
                }
                foreach (var expectedEmailType in corewebDefaults.CorewebEmailUpdtTypes)
                {
                    Assert.IsTrue(configuration.UpdatableEmailTypes.Contains(expectedEmailType));
                }
                foreach (var expectedPhoneType in corewebDefaults.CorewebPhoneUpdtTypes)
                {
                    Assert.IsTrue(configuration.UpdatablePhoneTypes.Contains(expectedPhoneType));
                }

                Assert.AreEqual(dflts.DfltsWebAdrelType, configuration.UpdatableAddressTypes.First());
            }

            [TestMethod]
            public async Task GetsUserProfileConfiguration_AllSettingsNull()
            {
                var corewebDefaultsNull = new CorewebDefaults()
                {
                    CorewebAddressViewTypes = null,
                    CorewebEmailViewTypes = null,
                    CorewebPhoneViewTypes = null
                };

                // Setup response to phoneType valcode read
                dataReaderMock.Setup(r => r.ReadRecordAsync<CorewebDefaults>("CORE.PARMS", "COREWEB.DEFAULTS", true))
                       .ReturnsAsync(corewebDefaultsNull);

                var configuration = await ConfigRepository.GetUserProfileConfigurationAsync();
                Assert.AreEqual(0, configuration.ViewableAddressTypes.Count());
                Assert.AreEqual(0, configuration.ViewablePhoneTypes.Count());
                Assert.AreEqual(0, configuration.ViewableEmailTypes.Count());
                Assert.IsNull(configuration.Text);
            }

            [TestMethod]
            public async Task GetsUserProfileConfiguration_AllTypesViewableAndUpdatable()
            {
                var dflts = new Dflts()
                {
                    DfltsWebAdrelType = "WB"
                };

                var corewebDefaults = new CorewebDefaults()
                {
                    CorewebAddressViewTypes = null,
                    CorewebEmailViewTypes = null,
                    CorewebPhoneViewTypes = null,
                    CorewebAllEmailViewable = "Y",
                    CorewebAllEmailUpdatable = "Y",
                    CorewebAllPhoneViewable = "Y",
                    CorewebAllAddrViewable = "Y",
                    CorewebEmailUpdtNoPerm = "Y",
                    CorewebPhoneUpdtNoPerm = "Y",
                    CorewebAddrUpdtNoPerm = "Y",
                    CorewebAddrUpdatable = "Y",
                };

                dataReaderMock.Setup(r => r.ReadRecordAsync<Dflts>("CORE.PARMS", "DEFAULTS", true));
                dataReaderMock.Setup(r => r.ReadRecordAsync<CorewebDefaults>("CORE.PARMS", "COREWEB.DEFAULTS", true))
                       .ReturnsAsync(corewebDefaults);

                var configuration = await ConfigRepository.GetUserProfileConfigurationAsync();
                Assert.AreEqual(0, configuration.ViewableAddressTypes.Count());
                Assert.AreEqual(0, configuration.ViewablePhoneTypes.Count());
                Assert.AreEqual(0, configuration.ViewableEmailTypes.Count());
                Assert.IsTrue(configuration.AllAddressTypesAreViewable);
                Assert.IsTrue(configuration.AllEmailTypesAreViewable);
                Assert.IsTrue(configuration.AllPhoneTypesAreViewable);
                Assert.IsTrue(configuration.AllEmailTypesAreUpdatable);
                Assert.IsTrue(configuration.CanUpdateEmailWithoutPermission);
                Assert.IsTrue(configuration.CanUpdatePhoneWithoutPermission);
            }

            [TestMethod]
            public async Task GetsUserProfileConfiguration2_AllTypesViewableAndUpdatable()
            {
                var dflts = new Dflts()
                {
                    DfltsWebAdrelType = "WB"
                };

                var corewebDefaults = new CorewebDefaults()
                {
                    CorewebAddressViewTypes = null,
                    CorewebEmailViewTypes = null,
                    CorewebPhoneViewTypes = null,
                    CorewebAllEmailViewable = "Y",
                    CorewebAllEmailUpdatable = "Y",
                    CorewebAllPhoneViewable = "Y",
                    CorewebAllAddrViewable = "Y",
                    CorewebEmailUpdtNoPerm = "Y",
                    CorewebPhoneUpdtNoPerm = "Y",
                    CorewebAddrUpdtNoPerm = "Y",
                    CorewebAddrUpdatable = "Y",
                    CorewebChoNameOption = "U",
                    CorewebNicknameOption = "V",
                    CorewebPronounOption = "N",
                    CorewebGenIdentOption = "U",
                };

                dataReaderMock.Setup(r => r.ReadRecordAsync<Dflts>("CORE.PARMS", "DEFAULTS", true));
                dataReaderMock.Setup(r => r.ReadRecordAsync<CorewebDefaults>("CORE.PARMS", "COREWEB.DEFAULTS", true))
                       .ReturnsAsync(corewebDefaults);

                var configuration = await ConfigRepository.GetUserProfileConfiguration2Async(null);
                Assert.AreEqual(0, configuration.ViewableAddressTypes.Count());
                Assert.AreEqual(0, configuration.ViewablePhoneTypes.Count());
                Assert.AreEqual(0, configuration.ViewableEmailTypes.Count());
                Assert.AreEqual(0, configuration.ChangeRequestAddressTypes.Count());
                Assert.IsTrue(configuration.AllAddressTypesAreViewable);
                Assert.IsTrue(configuration.AllEmailTypesAreViewable);
                Assert.IsTrue(configuration.AllPhoneTypesAreViewable);
                Assert.IsTrue(configuration.AllEmailTypesAreUpdatable);
                Assert.IsTrue(configuration.CanUpdateEmailWithoutPermission);
                Assert.IsTrue(configuration.CanUpdatePhoneWithoutPermission);
                Assert.AreEqual(UserProfileViewUpdateOption.Updatable, configuration.CanViewUpdateChosenName);
                Assert.AreEqual(UserProfileViewUpdateOption.Viewable, configuration.CanViewUpdateNickname);
                Assert.AreEqual(UserProfileViewUpdateOption.NotAllowed, configuration.CanViewUpdatePronoun);
                Assert.AreEqual(UserProfileViewUpdateOption.Updatable, configuration.CanViewUpdateGenderIdentity);
            }

        }

        #endregion

        [TestClass]
        public class ConfigurationRepository_GetEmergencyInformationConfigurationAsync : ConfigurationRepositoryTests
        {
            [TestMethod]
            public async Task GetEmergencyInformationConfiguration_RetrievesTrueValues()
            {
                var corewebDefaultsWithYes = new CorewebDefaults()
                {
                    CorewebEmerAllowOptout = "Y",
                    CorewebEmerHideHlthCond = "Y",
                    CorewebEmerHideOtherInfo = "Y",
                    CorewebEmerRequireContact = "Y"
                };

                dataReaderMock.Setup(r => r.ReadRecordAsync<CorewebDefaults>("CORE.PARMS", "COREWEB.DEFAULTS", true))
                       .ReturnsAsync(corewebDefaultsWithYes);

                var emerConfig = await ConfigRepository.GetEmergencyInformationConfigurationAsync();
                Assert.IsTrue(emerConfig.AllowOptOut);
                Assert.IsTrue(emerConfig.HideHealthConditions);
                Assert.IsTrue(emerConfig.HideOtherInformation);
                Assert.IsTrue(emerConfig.RequireContact);
            }

            [TestMethod]
            public async Task GetEmergencyInformationConfiguration_RetrievesFalseValues()
            {
                var corewebDefaultsWithNo = new CorewebDefaults()
                {
                    CorewebEmerAllowOptout = "N",
                    CorewebEmerHideHlthCond = "N",
                    CorewebEmerHideOtherInfo = "N",
                    CorewebEmerRequireContact = "N"
                };

                dataReaderMock.Setup(r => r.ReadRecordAsync<CorewebDefaults>("CORE.PARMS", "COREWEB.DEFAULTS", true))
                       .ReturnsAsync(corewebDefaultsWithNo);

                var emerConfig = await ConfigRepository.GetEmergencyInformationConfigurationAsync();
                Assert.IsFalse(emerConfig.AllowOptOut);
                Assert.IsFalse(emerConfig.HideHealthConditions);
                Assert.IsFalse(emerConfig.HideOtherInformation);
                Assert.IsFalse(emerConfig.RequireContact);
            }

            [TestMethod]
            public async Task GetEmergencyInformationConfiguration_NullValuesDefaultToFalse()
            {
                var corewebDefaultsWithNull = new CorewebDefaults()
                {
                    CorewebEmerAllowOptout = null,
                    CorewebEmerHideHlthCond = null,
                    CorewebEmerHideOtherInfo = null,
                    CorewebEmerRequireContact = null
                };

                dataReaderMock.Setup(r => r.ReadRecordAsync<CorewebDefaults>("CORE.PARMS", "COREWEB.DEFAULTS", true))
                       .ReturnsAsync(corewebDefaultsWithNull);

                var emerConfig = await ConfigRepository.GetEmergencyInformationConfigurationAsync();
                Assert.IsFalse(emerConfig.AllowOptOut);
                Assert.IsFalse(emerConfig.HideHealthConditions);
                Assert.IsFalse(emerConfig.HideOtherInformation);
                Assert.IsFalse(emerConfig.RequireContact);
            }
        }

        #region RestrictionConfiguration

        [TestClass]
        public class ConfigurationRepository_GetRestrictionConfigurationAsync : ConfigurationRepositoryTests
        {
            [TestMethod]
            public async Task GetRestrictionConfigurationTest()
            {
                var start = 0;
                var end = 100;
                var style = "I";

                var corewebDefaults = new CorewebDefaults()
                {
                    CorewebSeverityStart = new List<int?>(),
                    CorewebSeverityEnd = new List<int?>(),
                    CorewebStyle = new List<string>()
                };
                corewebDefaults.CorewebStyle.Add(style);
                corewebDefaults.CorewebSeverityStart.Add(start);
                corewebDefaults.CorewebSeverityEnd.Add(end);

                // Setup response to phoneType valcode read
                dataReaderMock.Setup(r => r.ReadRecordAsync<CorewebDefaults>("CORE.PARMS", "COREWEB.DEFAULTS", true))
                       .ReturnsAsync(corewebDefaults);

                var configuration = await ConfigRepository.GetRestrictionConfigurationAsync();
                Assert.AreEqual(1, configuration.Mapping.Count);
                Assert.AreEqual(start, configuration.Mapping[0].SeverityStart);
                Assert.AreEqual(end, configuration.Mapping[0].SeverityEnd);
                Assert.AreEqual("Information", configuration.Mapping[0].Style.ToString());
            }

            [TestMethod]
            public async Task GetEmptyRestrictionConfigurationTest()
            {

                var corewebDefaults = new CorewebDefaults()
                {
                    CorewebSeverityStart = new List<int?>(),
                    CorewebSeverityEnd = new List<int?>(),
                    CorewebStyle = new List<string>()
                };

                // Setup response to phoneType valcode read
                dataReaderMock.Setup(r => r.ReadRecordAsync<CorewebDefaults>("CORE.PARMS", "COREWEB.DEFAULTS", true))
                       .ReturnsAsync(corewebDefaults);

                var configuration = await ConfigRepository.GetRestrictionConfigurationAsync();
                Assert.AreEqual(0, configuration.Mapping.Count);
            }
        }

        #endregion

        #region PrivacyConfiguration

        [TestClass]
        public class ConfigurationRepository_GetPrivacyConfigurationAsync : ConfigurationRepositoryTests
        {
            [TestMethod]
            public async Task ConfigurationRepository_GetPrivacyConfigurationAsync_Valid()
            {
                var dflts = new Dflts()
                {
                    DfltsRecordDenialMsg = "Record not accesible due to a privacy request"
                };
                dataReaderMock.Setup(r => r.ReadRecordAsync<Dflts>("CORE.PARMS", "DEFAULTS", true))
                       .ReturnsAsync(dflts);

                var configuration = await ConfigRepository.GetPrivacyConfigurationAsync();
                Assert.AreEqual(dflts.DfltsRecordDenialMsg, configuration.RecordDenialMessage);
            }

            [TestMethod]
            [ExpectedException(typeof(ConfigurationException))]
            public async Task ConfigurationRepository_GetPrivacyConfigurationAsync_Null_Dflts()
            {
                Dflts dflts = null;
                dataReaderMock.Setup(r => r.ReadRecordAsync<Dflts>("CORE.PARMS", "DEFAULTS", true))
                       .ReturnsAsync(dflts);

                var configuration = await ConfigRepository.GetPrivacyConfigurationAsync();
            }
        }


        #endregion

        #region OrganizationalRelationshipConfiguration
        [TestClass]
        public class ConfigurationRepository_GetOrganizationalRelationshipConfigurationAsync : ConfigurationRepositoryTests
        {
            [TestMethod]
            public async Task ConfigurationRepository_GetOrganizationalRelationshipConfigurationAsync_Valid()
            {
                // mock data accessor PHONE.TYPES
                dataReaderMock.Setup<Task<ApplValcodes>>(a =>
                    a.ReadRecordAsync<ApplValcodes>("UT.VALCODES", "RELATIONSHIP.CATEGORIES", true))
                    .ReturnsAsync(new ApplValcodes()
                    {
                        ValInternalCode = new List<string>() { "NOTMGR", "MGR" },
                        ValExternalRepresentation = new List<string>() { "Not a manager", "Manager" },
                        ValActionCode1 = new List<string>() { "", "ORG" },
                        ValsEntityAssociation = new List<ApplValcodesVals>()
                        {
                            new ApplValcodesVals()
                            {
                                ValInternalCodeAssocMember = "NOTMGR",
                                ValExternalRepresentationAssocMember = "Not a manager",
                                ValActionCode1AssocMember = ""
                            },
                             new ApplValcodesVals()
                            {
                                ValInternalCodeAssocMember = "MGR",
                                ValExternalRepresentationAssocMember = "Manager",
                                ValActionCode1AssocMember = "ORG"
                            },
                        }
                    });
                var orgRelConfig = await ConfigRepository.GetOrganizationalRelationshipConfigurationAsync();
                Assert.AreEqual(1, orgRelConfig.RelationshipTypeCodeMapping.Keys.Count);
                Assert.AreEqual(1, orgRelConfig.RelationshipTypeCodeMapping.Values.Count);
                Assert.AreEqual(OrganizationalRelationshipType.Manager, orgRelConfig.RelationshipTypeCodeMapping.Keys.First());
                Assert.AreEqual("MGR", orgRelConfig.RelationshipTypeCodeMapping.Values.First().First());
            }
        }
        #endregion

        private ConfigurationRepository BuildValidRepository()
        {
            var transFactoryMock = new Mock<IColleagueTransactionFactory>();

            var loggerMock = new Mock<ILogger>();

            // Cache mocking
            var cacheProviderMock = new Mock<ICacheProvider>();
            var localCacheMock = new Mock<ObjectCache>();
            cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

            // Set up data accessor for mocking 
            dataReaderMock = new Mock<IColleagueDataReader>();

            // Set up data accessor for mocking 
            transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataReaderMock.Object);
            transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManagerMock.Object);

            //transManagerMock.Setup<GetIntegrationConfigResponse>(
            //    manager => manager.Execute<GetIntegrationConfigRequest, GetIntegrationConfigResponse>(
            //        It.IsAny<GetIntegrationConfigRequest>())).Returns<GetIntegrationConfigRequest>(request =>
            //        {
            //            switch (request.CdmIntegrationId)
            //            {
            //                case "TEST":
            //                    return cdmIntegrationResponses.Where(r => r.Recordkey == "TEST").FirstOrDefault();
            //                case "TEST2":
            //                    return cdmIntegrationResponses.Where(r => r.Recordkey == "TEST2").FirstOrDefault();
            //                default:
            //                    return new GetIntegrationConfigResponse()
            //                    {
            //                        ErrorMsgs = new List<string>() { "Record not found." }
            //                    };
            //            }
            //        });

            dataReaderMock.Setup<LdmDefaults>(
                reader => reader.ReadRecord<LdmDefaults>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns<string, string, bool>((file, record, replaceVm) =>
                {
                    return new LdmDefaults()
                    {
                        LdmdAddrDuplCriteria = "LDM.ADDR",
                        LdmdAddrTypeMapping = "LDM.ADDR.TYPE",
                        LdmdCourseActStatus = "A",
                        LdmdCourseInactStatus = "I",
                        LdmdEmailTypeMapping = "LDM.EMAIL",
                        LdmdPersonDuplCriteria = "LDM.PERSON",
                        LdmdSectionActStatus = "A",
                        LdmdSectionInactStatus = "I",
                        LdmdSubjDeptMapping = "LDM.SUBJ.DEPT",
                        Recordkey = "LDM.DEFAULTS",
                        LdmdCollDefaultsEntityAssociation = new List<LdmDefaultsLdmdCollDefaults>()
                            {
                                new LdmDefaultsLdmdCollDefaults()
                                {
                                    LdmdCollDefaultValueAssocMember = "A",
                                    LdmdCollFieldNameAssocMember = "B",
                                    LdmdCollFieldNumberAssocMember = 5,
                                    LdmdCollFileNameAssocMember = "COURSES"
                                }
                            },
                        LdmdPhoneTypeMappingEntityAssociation = new List<LdmDefaultsLdmdPhoneTypeMapping>()
                            {
                                new LdmDefaultsLdmdPhoneTypeMapping()
                                {
                                    LdmdLdmPhoneTypeAssocMember = "BUS",
                                    LdmdCollPhoneTypesAssocMember = "BU"
                                }
                            }
                    };
                }
            );

            dflts = new Dflts()
            {
                DfltsCampusCalendar = "MAIN",
                DfltsWebAdrelType = "WB"
            };

            dataReaderMock.Setup<Dflts>(
                reader => reader.ReadRecord<Dflts>("CORE.PARMS", "DEFAULTS", It.IsAny<bool>()))
                .Returns<string, string, bool>((file, record, replaceVm) =>
                {
                    return dflts;
                }
            );

            dataReaderMock.Setup<Task<HrwebDefaults>>(
                datareader => datareader.ReadRecordAsync<HrwebDefaults>(It.IsAny<string>(), It.IsAny<string>(), true))
                .Returns(() =>
                {
                    return Task.FromResult(expectedHrWebDefaults);
                });

            corewebDefaults = new CorewebDefaults()
            {
                CorewebAddressViewTypes = new List<string>() { "HO", "B" },
                CorewebEmailViewTypes = new List<string>() { "WEB", "I" },
                CorewebPhoneViewTypes = new List<string>() { "H", "BUS" },
                CorewebEmailUpdtTypes = new List<string>() { "WEB" },
                CorewebPhoneUpdtTypes = new List<string>() { "H" },
                CorewebUserProfileText = "Please contact the administration with any changes.",
                CorewebAddrUpdatable = "Y",
                CorewebAllEmailUpdatable = "N",
                CorewebAddrUpdtNoPerm = "Y",
                CorewebAllAddrViewable = "N",
                CorewebAllPhoneViewable = "N",
                CorewebAllEmailViewable = "N",
                CorewebEmailUpdtNoPerm = "Y",
                CorewebPhoneUpdtNoPerm = "Y",
                CorewebEmerAllowOptout = "Y",
                CorewebEmerHideHlthCond = "Y",
                CorewebEmerRequireContact = "Y"
            };
            dataReaderMock.Setup(r => r.ReadRecordAsync<CorewebDefaults>("CORE.PARMS", "COREWEB.DEFAULTS", true))
                   .ReturnsAsync(corewebDefaults);

            dataReaderMock.Setup<Task<QtdYtdParameterW2>>(
                dataReader => dataReader.ReadRecordAsync<QtdYtdParameterW2>(It.IsAny<string>(), It.IsAny<string>(), true))
                .Returns(() =>
                {
                    // Make sure we build the association
                    expectedQtdYtdParameterW2.buildAssociations();
                    return Task.FromResult(expectedQtdYtdParameterW2);
                });

            dataReaderMock.Setup<Task<QtdYtdParameter1095C>>(
                dataReader => dataReader.ReadRecordAsync<QtdYtdParameter1095C>(It.IsAny<string>(), It.IsAny<string>(), true))
                .Returns(() =>
                {
                    // Make sure we build the association
                    expectedQtdYtdParameter1095C.buildAssociations();
                    return Task.FromResult(expectedQtdYtdParameter1095C);
                });

            dataReaderMock.Setup<Task<Parm1098>>(
                dataReader => dataReader.ReadRecordAsync<Parm1098>(It.IsAny<string>(), It.IsAny<string>(), true))
                .Returns(() =>
                {
                    // Make sure we build the association
                    expectedQtdYtdParameter1095C.buildAssociations();
                    return Task.FromResult(parm1098DataContract);
                });

            dataReaderMock.Setup<Task<ParmT4>>(dataReader => dataReader.ReadRecordAsync<ParmT4>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(() =>
                {
                    return Task.FromResult(parmT4DataContract);
                });

            dataReaderMock.Setup<Task<ParmT4a>>(dataReader => dataReader.ReadRecordAsync<ParmT4a>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(parmT4ADataContract);
            });

            dataReaderMock.Setup<Task<CnstRptParms>>(dataReader => dataReader.ReadRecordAsync<CnstRptParms>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(cnstRptParmsContract);
            });

            dataReaderMock.Setup(x => x.BulkReadRecordAsync<TaxForm1098Years>(It.IsAny<string>(), true)).Returns(() =>
                {
                    return Task.FromResult(taxForm1098YearsContracts);
                });

            dataReaderMock.Setup(x => x.ReadRecordAsync<TaxFormStatus>(It.IsAny<string>(), true)).Returns(() =>
                {
                    return Task.FromResult(taxFormStatusContract);
                });

            dataReaderMock.Setup(x => x.ReadRecordAsync<Parm1098>("ST.PARMS", "PARM.1098", true)).Returns(() =>
                {
                    return Task.FromResult(parm1098DataContract);
                });

            dataReaderMock.Setup(x => x.ReadRecordAsync<QtdYtdParameterT4>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(() =>
            {

                return Task.FromResult(expectedQtdYtdParameterT4);
            });

            cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));
            // Construct referenceData repository
            ConfigRepository = new ConfigurationRepository(cacheProvider, transFactoryMock.Object, FakeApiSettingsWithGmtColleagueTimeZone, logger, colleagueSettings);

            return ConfigRepository;
        }

        #region backup configuration

        [TestMethod]
        public async Task ConfigurationRepository_AddBackupConfigurationAsync()
        {
            // Arrange
            WriteBackupConfigDataRequest fakeWriteRequest = new WriteBackupConfigDataRequest();
            var fakeWriteResponse = new WriteBackupConfigDataResponse()
            {
                ConfigDataId = "56c1fb34-9e3e-49a0-b2b0-60751a877855",
                Error = ""
            };

            transManagerMock.Setup(
                i => i.ExecuteAsync<WriteBackupConfigDataRequest, WriteBackupConfigDataResponse>
                (It.IsAny<WriteBackupConfigDataRequest>())).ReturnsAsync(fakeWriteResponse);

            ReadBackupConfigDataRequest fakeReadRequest = new ReadBackupConfigDataRequest();

            transManagerMock.Setup(
                i => i.ExecuteAsync<ReadBackupConfigDataRequest, ReadBackupConfigDataResponse>
                (It.IsAny<ReadBackupConfigDataRequest>())).ReturnsAsync(FakeBackupConfigReadResponse);

            ConfigRepository = new ConfigurationRepository(
                cacheProvider, transFactory, FakeApiSettingsWithGmtColleagueTimeZone, logger, colleagueSettings);

            // Act
            var actual = await ConfigRepository.AddBackupConfigurationAsync(FakeBackupConfigurationData);

            // Assert            Assert.IsNotNull(actual);
            Assert.AreEqual(FakeBackupConfigurationData.Id, actual.Id);
            Assert.AreEqual(FakeBackupConfigurationData.Namespace, actual.Namespace);
            Assert.AreEqual(FakeBackupConfigurationData.ProductId, actual.ProductId);
            Assert.AreEqual(FakeBackupConfigurationData.ProductVersion, actual.ProductVersion);
            Assert.AreEqual(FakeBackupConfigurationData.ConfigData, actual.ConfigData);
            Assert.AreEqual(FakeBackupConfigurationData.ConfigVersion, actual.ConfigVersion);
            Assert.AreEqual(FakeBackupConfigurationData.CreatedDateTime, actual.CreatedDateTime);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ConfigurationRepository_AddBackupConfigurationAsync_CtxError()
        {
            // Arrange
            WriteBackupConfigDataRequest fakeWriteRequest = new WriteBackupConfigDataRequest();
            var fakeWriteResponse = new WriteBackupConfigDataResponse()
            {
                ConfigDataId = "",
                Error = "Something went wrong"
            };
            transManagerMock.Setup(
                i => i.ExecuteAsync<WriteBackupConfigDataRequest, WriteBackupConfigDataResponse>
                (It.IsAny<WriteBackupConfigDataRequest>())).ReturnsAsync(fakeWriteResponse);

            ConfigRepository = new ConfigurationRepository(
                cacheProvider, transFactory, FakeApiSettingsWithGmtColleagueTimeZone, logger, colleagueSettings);

            // Act
            var actual = await ConfigRepository.AddBackupConfigurationAsync(FakeBackupConfigurationData);
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ConfigurationRepository_AddBackupConfigurationAsync_Nullarg()
        {
            ConfigRepository = new ConfigurationRepository(
                cacheProvider, transFactory, FakeApiSettingsWithGmtColleagueTimeZone, logger, colleagueSettings);
            // Act
            var actual = await ConfigRepository.AddBackupConfigurationAsync(null);
        }


        [TestMethod]
        public async Task ConfigurationRepository_GetBackupConfigurationAsync()
        {
            // Arrange
            ReadBackupConfigDataRequest fakeReadRequest = new ReadBackupConfigDataRequest();

            transManagerMock.Setup(
                i => i.ExecuteAsync<ReadBackupConfigDataRequest, ReadBackupConfigDataResponse>
                (It.IsAny<ReadBackupConfigDataRequest>())).ReturnsAsync(FakeBackupConfigReadResponse);

            ConfigRepository = new ConfigurationRepository(
                cacheProvider, transFactory, FakeApiSettingsWithGmtColleagueTimeZone, logger, colleagueSettings);

            // Act
            var actualSet = await ConfigRepository.GetBackupConfigurationByIdsAsync(new List<string>() { "56c1fb34-9e3e-49a0-b2b0-60751a877855" });
            var actual = actualSet.FirstOrDefault();
            // Assert            Assert.IsNotNull(actual);
            Assert.AreEqual(FakeBackupConfigurationData.Id, actual.Id);
            Assert.AreEqual(FakeBackupConfigurationData.Namespace, actual.Namespace);
            Assert.AreEqual(FakeBackupConfigurationData.ProductId, actual.ProductId);
            Assert.AreEqual(FakeBackupConfigurationData.ProductVersion, actual.ProductVersion);
            Assert.AreEqual(FakeBackupConfigurationData.ConfigData, actual.ConfigData);
            Assert.AreEqual(FakeBackupConfigurationData.ConfigVersion, actual.ConfigVersion);
            Assert.AreEqual(FakeBackupConfigurationData.CreatedDateTime, actual.CreatedDateTime);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ConfigurationRepository_GetBackupConfigurationAsync_CtxError()
        {
            // Arrange
            ReadBackupConfigDataRequest fakeReadRequest = new ReadBackupConfigDataRequest();
            var fakeReadResponse = new ReadBackupConfigDataResponse()
            {
                Error = "Something went wrong"
            };
            transManagerMock.Setup(
                i => i.ExecuteAsync<ReadBackupConfigDataRequest, ReadBackupConfigDataResponse>
                (It.IsAny<ReadBackupConfigDataRequest>())).ReturnsAsync(fakeReadResponse);

            ConfigRepository = new ConfigurationRepository(
                cacheProvider, transFactory, FakeApiSettingsWithGmtColleagueTimeZone, logger, colleagueSettings);

            // Act
            var actual = await ConfigRepository.GetBackupConfigurationByIdsAsync(new List<string>() { "56c1fb34-9e3e-49a0-b2b0-60751a877855" });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task ConfigurationRepository_GetBackupConfigurationAsync_NullIds()
        {
            ConfigRepository = new ConfigurationRepository(
                cacheProvider, transFactory, FakeApiSettingsWithGmtColleagueTimeZone, logger, colleagueSettings);
            // Act
            var actual = await ConfigRepository.GetBackupConfigurationByIdsAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ConfigurationRepository_GetBackupConfigurationAsync_NullNamespace()
        {
            ConfigRepository = new ConfigurationRepository(
                cacheProvider, transFactory, FakeApiSettingsWithGmtColleagueTimeZone, logger, colleagueSettings);
            // Act
            var actual = await ConfigRepository.GetBackupConfigurationByNamespaceAsync(null);
        }

        #endregion

        #region SelfServiceConfiguration

        [TestClass]
        public class ConfigurationRepository_GetSelfServiceConfigurationAsync : ConfigurationRepositoryTests
        {
            [TestMethod]
            public async Task ConfigurationRepository_GetSelfServiceConfigurationAsync_Valid_DfltsEmailAllAlwaysCopy_Y()
            {
                var dflts = new Dflts()
                {
                    DfltsEmailAllAlwaysCopy = "Y"
                };
                dataReaderMock.Setup(r => r.ReadRecordAsync<Dflts>("CORE.PARMS", "DEFAULTS", true))
                       .ReturnsAsync(dflts);
                var configuration = await ConfigRepository.GetSelfServiceConfigurationAsync();
                Assert.IsTrue(configuration.AlwaysUseClipboardForBulkMailToLinks);
            }

            [TestMethod]
            public async Task ConfigurationRepository_GetSelfServiceConfigurationAsync_Valid_DfltsEmailAllAlwaysCopy_null()
            {
                var dflts = new Dflts()
                {
                    DfltsEmailAllAlwaysCopy = null
                };
                dataReaderMock.Setup(r => r.ReadRecordAsync<Dflts>("CORE.PARMS", "DEFAULTS", true))
                       .ReturnsAsync(dflts);
                var configuration = await ConfigRepository.GetSelfServiceConfigurationAsync();
                Assert.IsFalse(configuration.AlwaysUseClipboardForBulkMailToLinks);
            }

            [TestMethod]
            public async Task ConfigurationRepository_GetSelfServiceConfigurationAsync_Valid_DfltsEmailAllAlwaysCopy_N()
            {
                var dflts = new Dflts()
                {
                    DfltsEmailAllAlwaysCopy = "N"
                };
                dataReaderMock.Setup(r => r.ReadRecordAsync<Dflts>("CORE.PARMS", "DEFAULTS", true))
                       .ReturnsAsync(dflts);
                var configuration = await ConfigRepository.GetSelfServiceConfigurationAsync();
                Assert.IsFalse(configuration.AlwaysUseClipboardForBulkMailToLinks);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task ConfigurationRepository_GetSelfServiceConfigurationAsync_Null_Dflts()
            {
                Dflts dflts = null;
                dataReaderMock.Setup(r => r.ReadRecordAsync<Dflts>("CORE.PARMS", "DEFAULTS", true))
                       .ReturnsAsync(dflts);

                var configuration = await ConfigRepository.GetSelfServiceConfigurationAsync();
            }
        }
        #endregion

        #region Configuration for RequiredDocuments
        [TestClass]
        public class ConfigurationRepository_GetRequiredDocumentConfigurationAsync : ConfigurationRepositoryTests
        {
            [TestMethod]
            public async Task GetRequiredDocumentConfiguration_CoreWebDefaultFailure()
            {
                var association = new List<OfficeCollectionMapOfcomap>()
                {
                    new OfficeCollectionMapOfcomap() { OfcoOfficeCodesAssocMember = "OFFICEA", OfcoCollectionIdsAssocMember = "COLLECTIONA"},
                    new OfficeCollectionMapOfcomap() { OfcoOfficeCodesAssocMember = "", OfcoCollectionIdsAssocMember = "COLLECTIONC"},
                    new OfficeCollectionMapOfcomap() { OfcoOfficeCodesAssocMember = "OFFICEB", OfcoCollectionIdsAssocMember = "COLLECTIONB"},
                    new OfficeCollectionMapOfcomap() { OfcoOfficeCodesAssocMember = "OFFICEB", OfcoCollectionIdsAssocMember = ""},
                    new OfficeCollectionMapOfcomap() { OfcoOfficeCodesAssocMember = "OFFICEC", OfcoCollectionIdsAssocMember = "COLLECTIONC"},
                    new OfficeCollectionMapOfcomap() { OfcoOfficeCodesAssocMember = "OFFICEC", OfcoCollectionIdsAssocMember = "COLLECTIONC"},
                };

                var officeMap = new OfficeCollectionMap()
                {
                    OfcoDefaultCollection = "NO_OFFICE_CODE_COLLECTION",
                    OfcoDfltOfficeCollection = "UNMAPPED_COLLECTION",
                    OfcomapEntityAssociation = association
                };

                // Setup response to OFFICE.COLLECTION.MAP read
                dataReaderMock.Setup(r => r.ReadRecordAsync<OfficeCollectionMap>("CORE.PARMS", "OFFICE.COLLECTION.MAP", true))
                       .ReturnsAsync(officeMap);

                // Setup response to CoreWebDefaults  read
                dataReaderMock.Setup(r => r.ReadRecordAsync<CorewebDefaults>("CORE.PARMS", "COREWEB.DEFAULTS", true))
                       .ThrowsAsync(new Exception());

                var configuration = await ConfigRepository.GetRequiredDocumentConfigurationAsync();
                Assert.IsNull(configuration);
               
            }

            [TestMethod]
            public async Task GetsRequiredDocumentConfiguration_AllSettingsNullOrBlank()
            {
                var corewebDefaultsNull = new CorewebDefaults()
                {
                    CorewebSuppressInstance = null,
                    CorewebDocumentsSort1 = null,
                    CorewebDocumentsSort2 = null,
                    CorewebBlankStatusText = "",
                    CorewebBlankDueDateText = ""
                };


                // Setup response to CoreWebDefaults  read
                dataReaderMock.Setup(r => r.ReadRecordAsync<CorewebDefaults>("CORE.PARMS", "COREWEB.DEFAULTS", true))
                       .ReturnsAsync(corewebDefaultsNull);
                dataReaderMock.Setup(r => r.ReadRecordAsync<OfficeCollectionMap>("CORE.PARMS", "OFFICE.COLLECTION.MAP", true))
                        .ReturnsAsync(null);

                var configuration = await ConfigRepository.GetRequiredDocumentConfigurationAsync();
                Assert.AreEqual(false, configuration.SuppressInstance);
                Assert.AreEqual(WebSortField.Status, configuration.PrimarySortField);
                Assert.AreEqual(WebSortField.OfficeDescription, configuration.SecondarySortField);
                Assert.AreEqual("", configuration.TextForBlankStatus);
                Assert.AreEqual("", configuration.TextForBlankDueDate);
                Assert.IsNull(configuration.RequiredDocumentCollectionMapping.RequestsWithoutOfficeCodeCollection);
                Assert.IsNull(configuration.RequiredDocumentCollectionMapping.UnmappedOfficeCodeCollection);
                Assert.AreEqual(0, configuration.RequiredDocumentCollectionMapping.OfficeCodeMapping.Count);
            }

            [TestMethod]
            public async Task GetsRequiredDocumentConfiguration_AllValuesRetrieved()
            {
                var dflts = new Dflts()
                {
                    DfltsWebAdrelType = "WB"
                };

                var corewebDefaults = new CorewebDefaults()
                {
                    CorewebSuppressInstance = "Y",
                    CorewebDocumentsSort1 = "STATDATE",
                    CorewebDocumentsSort2 = "DESC",
                    CorewebBlankStatusText = "Asap",
                    CorewebBlankDueDateText = "Due"
                };

                var association = new List<OfficeCollectionMapOfcomap>()
                {
                    new OfficeCollectionMapOfcomap() { OfcoOfficeCodesAssocMember = "OFFICEA", OfcoCollectionIdsAssocMember = "COLLECTIONA"},
                    new OfficeCollectionMapOfcomap() { OfcoOfficeCodesAssocMember = "", OfcoCollectionIdsAssocMember = "COLLECTIONC"},
                    new OfficeCollectionMapOfcomap() { OfcoOfficeCodesAssocMember = "OFFICEB", OfcoCollectionIdsAssocMember = "COLLECTIONB"},
                    new OfficeCollectionMapOfcomap() { OfcoOfficeCodesAssocMember = "OFFICEB", OfcoCollectionIdsAssocMember = ""},
                    new OfficeCollectionMapOfcomap() { OfcoOfficeCodesAssocMember = "OFFICEC", OfcoCollectionIdsAssocMember = "COLLECTIONC"},
                    new OfficeCollectionMapOfcomap() { OfcoOfficeCodesAssocMember = "OFFICEC", OfcoCollectionIdsAssocMember = "COLLECTIONC"},
                };

                var officeMap = new OfficeCollectionMap()
                {
                    OfcoDefaultCollection = "NO_OFFICE_CODE_COLLECTION",
                    OfcoDfltOfficeCollection = "UNMAPPED_COLLECTION",
                    OfcomapEntityAssociation = association
                };

                dataReaderMock.Setup(r => r.ReadRecordAsync<Dflts>("CORE.PARMS", "DEFAULTS", true));
                dataReaderMock.Setup(r => r.ReadRecordAsync<CorewebDefaults>("CORE.PARMS", "COREWEB.DEFAULTS", true))
                       .ReturnsAsync(corewebDefaults);

                // Setup response to OFFICE.COLLECTION.MAP read
                dataReaderMock.Setup(r => r.ReadRecordAsync<OfficeCollectionMap>("CORE.PARMS", "OFFICE.COLLECTION.MAP", true))
                       .ReturnsAsync(officeMap);

                var configuration = await ConfigRepository.GetRequiredDocumentConfigurationAsync();
                Assert.AreEqual(true, configuration.SuppressInstance);
                Assert.AreEqual(WebSortField.StatusDate, configuration.PrimarySortField);
                Assert.AreEqual(WebSortField.Description, configuration.SecondarySortField);
                Assert.AreEqual("Asap", configuration.TextForBlankStatus);
                Assert.AreEqual("Due", configuration.TextForBlankDueDate);
                Assert.IsNotNull(configuration.RequiredDocumentCollectionMapping);
                var actualMapping = configuration.RequiredDocumentCollectionMapping;
                Assert.AreEqual(3, actualMapping.OfficeCodeMapping.Count);
                var officeMapping = actualMapping.OfficeCodeMapping;
                Assert.AreEqual("OFFICEA", officeMapping[0].OfficeCode);
                Assert.AreEqual("COLLECTIONA", officeMapping[0].AttachmentCollection);
                Assert.AreEqual("OFFICEB", officeMapping[1].OfficeCode);
                Assert.AreEqual("COLLECTIONB", officeMapping[1].AttachmentCollection);
                Assert.AreEqual("OFFICEC", officeMapping[2].OfficeCode);
                Assert.AreEqual("COLLECTIONC", officeMapping[2].AttachmentCollection);
            }

            #endregion
        }

        #region Configuration for SessionConfiguration
        [TestClass]
        public class ConfigurationRepository_GetSessionConfigurationAsync : ConfigurationRepositoryTests
        {
            [TestMethod]
            public async Task GetSessionConfiguration_AllSettingsBlank_BecomeFalse()
            {
                var response = new GetSessionConfigurationResponse()
                {
                    PasswordResetEnabled = "",
                    UsernameRecoveryEnabled = "",
                    ErrorOccurred = "0",
                    ErrorMessage = ""
                };

                transManagerMock.Setup(e => e.ExecuteAnonymousAsync<GetSessionConfigurationRequest, GetSessionConfigurationResponse>(It.IsAny<GetSessionConfigurationRequest>()))
                    .ReturnsAsync(response);

                var sessionConfiguration = await ConfigRepository.GetSessionConfigurationAsync();
                Assert.IsFalse(sessionConfiguration.PasswordResetEnabled);
                Assert.IsFalse(sessionConfiguration.UsernameRecoveryEnabled);
            }

            [TestMethod]
            public async Task GetSessionConfiguration_AllSettingsN_BecomeFalse()
            {
                var response = new GetSessionConfigurationResponse()
                {
                    PasswordResetEnabled = "N",
                    UsernameRecoveryEnabled = "N",
                    ErrorOccurred = "0",
                    ErrorMessage = ""
                };

                transManagerMock.Setup(e => e.ExecuteAnonymousAsync<GetSessionConfigurationRequest, GetSessionConfigurationResponse>(It.IsAny<GetSessionConfigurationRequest>()))
                    .ReturnsAsync(response);

                var sessionConfiguration = await ConfigRepository.GetSessionConfigurationAsync();
                Assert.IsFalse(sessionConfiguration.PasswordResetEnabled);
                Assert.IsFalse(sessionConfiguration.UsernameRecoveryEnabled);
            }

            [TestMethod]
            public async Task GetSessionConfiguration_AllSettingsY_BecomeTrue()
            {
                var response = new GetSessionConfigurationResponse()
                {
                    PasswordResetEnabled = "Y",
                    UsernameRecoveryEnabled = "Y",
                    ErrorOccurred = "0",
                    ErrorMessage = ""
                };

                transManagerMock.Setup(e => e.ExecuteAnonymousAsync<GetSessionConfigurationRequest, GetSessionConfigurationResponse>(It.IsAny<GetSessionConfigurationRequest>()))
                    .ReturnsAsync(response);

                var sessionConfiguration = await ConfigRepository.GetSessionConfigurationAsync();
                Assert.IsTrue(sessionConfiguration.PasswordResetEnabled);
                Assert.IsTrue(sessionConfiguration.UsernameRecoveryEnabled);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task GetSessionConfiguration_ErrorOccurred_ThrowsException()
            {
                var response = new GetSessionConfigurationResponse()
                {
                    PasswordResetEnabled = "",
                    UsernameRecoveryEnabled = "",
                    ErrorOccurred = "1",
                    ErrorMessage = "Error message from transaction"
                };

                transManagerMock.Setup(e => e.ExecuteAnonymousAsync<GetSessionConfigurationRequest, GetSessionConfigurationResponse>(It.IsAny<GetSessionConfigurationRequest>()))
                    .ReturnsAsync(response);

                var sessionConfiguration = await ConfigRepository.GetSessionConfigurationAsync();
            }

            #endregion
        }
    }
}