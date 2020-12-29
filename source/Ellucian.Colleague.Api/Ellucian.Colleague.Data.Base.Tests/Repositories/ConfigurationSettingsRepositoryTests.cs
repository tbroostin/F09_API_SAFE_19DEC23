//Copyright 2019-2020 Ellucian Company L.P. and its affiliates.  
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Entities;
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
using System.Threading;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Base.Tests.Repositories
{
    [TestClass]
    public class ConfigurationSettingsRepositoryTests
    {
        /// <summary>
        /// Test class for ConfigurationSettings codes
        /// </summary>
        [TestClass]
        public class ConfigurationSettingsTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<ConfigurationSettings> _configurationSettingsCollection;
            string codeItemName;
            Collection<ElfDuplCriteria> elfDuplCriteria;
            Collection<DataContracts.Person> personCollection;

            ConfigurationSettingsRepository configurationSettingsRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build responses used for mocking
                _configurationSettingsCollection = new List<ConfigurationSettings>()
                {
                    new Domain.Base.Entities.ConfigurationSettings("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "1", "Person Match Criteria")
                    {
                        EthosResources = new List<string>() { "persons" },
                        SourceTitle = "Integration Person Matching",
                        SourceValue = "INTG.PERSON",
                        FieldHelp = "Long Description for field help.",
                        EntityName = "ELF.DUPL.CRITERIA",
                        FieldName = "LDMD.PERSON.DUPL.CRITERIA"
                    },
                    new Domain.Base.Entities.ConfigurationSettings("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "2", "Address Match Criteria")
                    {
                        EthosResources = new List<string>() { "addresses" },
                        SourceTitle = "Integration Address Matching",
                        SourceValue = "INTG.ADDRESS",
                        FieldHelp = "Long Description for field help.",
                        EntityName = "ELF.DUPL.CRITERIA",
                        FieldName = "LDMD.ADDR.DUPL.CRITERIA"
                    },
                    new Domain.Base.Entities.ConfigurationSettings("d2253ac7-9931-4560-b42f-1fccd43c952e", "3", "Check Faculty Load")
                    {
                        EthosResources = new List<string>() { "section-instructors" },
                        SourceTitle = "Yes",
                        SourceValue = "Y",
                        FieldHelp = "Long Description for field help.",
                        FieldName = "LDMD.CHECK.FACULTY.LOAD"
                    },
                    new Domain.Base.Entities.ConfigurationSettings("7a2bf6b5-cdcd-4c8f-b5d8-4664bf5b3fbc", "4", "Registration Users ID")
                    {
                        EthosResources = new List<string>() { "section-registrations" },
                        SourceTitle = "WEBREG",
                        SourceValue = "WEBREG",
                        FieldHelp = "Long Description for field help.",
                        EntityName = "REG.USERS",
                        FieldName = "LDMD.REG.USERS.ID"
                    },
                    new Domain.Base.Entities.ConfigurationSettings("849e6a7c-6cd4-4f98-8a73-ab0aa8875f0d", "5", "Student Payment Cashier")
                    {
                        EthosResources = new List<string>() { "student-payments" },
                        SourceTitle = "Steven Magnusson",
                        SourceValue = "0003582",
                        FieldHelp = "Long Description for field help.",
                        EntityName = "CASHIERS",
                        FieldName = "LDMD.CASHIER"
                    },
                    new Domain.Base.Entities.ConfigurationSettings("d2253ac7-9931-2289-b42f-1fccd43c952e", "6", "Elevate Posting Performed in Colleague")
                    {
                        EthosResources = new List<string>() { "student-charges", "student-payments" },
                        SourceTitle = "No",
                        SourceValue = "N",
                        FieldHelp = "Long Description for field help.",
                        FieldName = "LDMD.CHECK.POSTING.PERFORMED"
                    },
                    new Domain.Base.Entities.ConfigurationSettings("d8836ac7-9931-2289-b42f-1fccd43c952e", "7", "Principal Investigator Contact Role")
                    {
                        EthosResources = new List<string>() { "grants" },
                        SourceTitle = "Interviewer",
                        SourceValue = "INTVR",
                        FieldHelp = "Long Description for field help.",
                        EntityName = "CORE.VALCODES",
                        FieldName = "LDMD.PRIN.INVESTIGATOR.ROLE",
                        ValcodeTableName = "CONTACT.ROLES"
                    },
                    new Domain.Base.Entities.ConfigurationSettings("d9926ac7-8374-2289-b42f-1fccd43c952e", "8", "Mapping Update Rules")
                    {
                        EthosResources = new List<string>() { "mapping-settings" },
                        SourceTitle = "Update ethos value",
                        SourceValue = "ethos",
                        FieldHelp = "Long Description for field help.",
                        FieldName = "LDMD.MAPPING.CONTROL"
                    },
                    new Domain.Base.Entities.ConfigurationSettings("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0f", "9", "Prospect Match Criteria")
                    {
                        EthosResources = new List<string>() { "person-matching-requestss" },
                        SourceTitle = "Person Match Request Prospect",
                        SourceValue = "INTG.PMR.PROSPECT",
                        FieldHelp = "Long Description for field help.",
                        EntityName = "ELF.DUPL.CRITERIA",
                        FieldName = "LDMD.PROSPECT.DUPL.CRITERIA"
                    },
                    
                    new Domain.Base.Entities.ConfigurationSettings("7125f63a-8f8c-4d41-9752-a2eced5bc8bd", "10", "Relation Match Request Criteria")
                    {
                        EthosResources = new List<string>() { "person-matching-requests" },
                        SourceTitle = "Person Match Request Relation",
                        SourceValue = "INTG.PMR.RELATION",
                        FieldHelp = "Long Description for field help.",
                        EntityName = "ELF.DUPL.CRITERIA",
                        FieldName = "LDMD.RELATION.DUPL.CRITERIA"
                    },
                    
                    new Domain.Base.Entities.ConfigurationSettings("c289fc49-408c-49e3-9197-010b53ceece9", "11", "Vendor Contact Match Request Criteria")
                    {
                        EthosResources = new List<string>() { "person-matching-requests" },
                        SourceTitle = "Person Match Request Vendor Contact",
                        SourceValue = "INTG.PMR.VENCONTACT",
                        FieldHelp = "Long Description for field help.",
                        EntityName = "ELF.DUPL.CRITERIA",
                        FieldName = "LDMD.VENCONTACT.DUP.CRITERIA"
                    }
                };

                // Build repository
                configurationSettingsRepo = BuildValidConfigurationSettingsRepository();
                codeItemName = configurationSettingsRepo.BuildFullCacheKey("AllIntgConfigSettings");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                _configurationSettingsCollection = null;
                configurationSettingsRepo = null;
            }

            [TestMethod]
            public async Task GetsConfigurationSettingsCacheAsync()
            {
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(_configurationSettingsCollection, new SemaphoreSlim(1, 1)));

                var result = await configurationSettingsRepo.GetConfigurationSettingsAsync(false);

                for (int i = 0; i < _configurationSettingsCollection.Count(); i++)
                {
                    Assert.AreEqual(_configurationSettingsCollection.ElementAt(i).Guid, result.ElementAt(i).Guid);
                    Assert.AreEqual(_configurationSettingsCollection.ElementAt(i).Code, result.ElementAt(i).Code);
                    Assert.AreEqual(_configurationSettingsCollection.ElementAt(i).Description, result.ElementAt(i).Description);
                    Assert.AreEqual(_configurationSettingsCollection.ElementAt(i).EntityName, result.ElementAt(i).EntityName);
                    Assert.AreEqual(_configurationSettingsCollection.ElementAt(i).EthosResources.FirstOrDefault(), result.ElementAt(i).EthosResources.FirstOrDefault());
                    Assert.AreEqual(_configurationSettingsCollection.ElementAt(i).FieldHelp, result.ElementAt(i).FieldHelp);
                    Assert.AreEqual(_configurationSettingsCollection.ElementAt(i).FieldName, result.ElementAt(i).FieldName);
                    Assert.AreEqual(_configurationSettingsCollection.ElementAt(i).MultipleValues, result.ElementAt(i).MultipleValues);
                    Assert.AreEqual(_configurationSettingsCollection.ElementAt(i).SourceTitle, result.ElementAt(i).SourceTitle);
                    Assert.AreEqual(_configurationSettingsCollection.ElementAt(i).SourceValue, result.ElementAt(i).SourceValue);
                    Assert.AreEqual(_configurationSettingsCollection.ElementAt(i).ValcodeTableName, result.ElementAt(i).ValcodeTableName);
                }
            }

            [TestMethod]
            public async Task GetsConfigurationSettingsNonCacheAsync()
            {
                var result = await configurationSettingsRepo.GetConfigurationSettingsAsync(true);

                for (int i = 0; i < _configurationSettingsCollection.Count(); i++)
                {
                    Assert.AreEqual(_configurationSettingsCollection.ElementAt(i).Guid, result.ElementAt(i).Guid);
                    Assert.AreEqual(_configurationSettingsCollection.ElementAt(i).Code, result.ElementAt(i).Code);
                    Assert.AreEqual(_configurationSettingsCollection.ElementAt(i).Description, result.ElementAt(i).Description);
                    Assert.AreEqual(_configurationSettingsCollection.ElementAt(i).EntityName, result.ElementAt(i).EntityName);
                    Assert.AreEqual(_configurationSettingsCollection.ElementAt(i).EthosResources.FirstOrDefault(), result.ElementAt(i).EthosResources.FirstOrDefault());
                    Assert.AreEqual(_configurationSettingsCollection.ElementAt(i).FieldHelp, result.ElementAt(i).FieldHelp);
                    Assert.AreEqual(_configurationSettingsCollection.ElementAt(i).FieldName, result.ElementAt(i).FieldName);
                    Assert.AreEqual(_configurationSettingsCollection.ElementAt(i).MultipleValues, result.ElementAt(i).MultipleValues);
                    Assert.AreEqual(_configurationSettingsCollection.ElementAt(i).SourceTitle, result.ElementAt(i).SourceTitle);
                    Assert.AreEqual(_configurationSettingsCollection.ElementAt(i).SourceValue, result.ElementAt(i).SourceValue);
                    Assert.AreEqual(_configurationSettingsCollection.ElementAt(i).ValcodeTableName, result.ElementAt(i).ValcodeTableName);
                }
            }

            [TestMethod]
            public async Task GetsConfigurationSettingsByGuidCacheAsync()
            {
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(_configurationSettingsCollection, new SemaphoreSlim(1, 1)));

                foreach (var expected in _configurationSettingsCollection)
                {
                    var result = await configurationSettingsRepo.GetConfigurationSettingsByGuidAsync(expected.Guid, false);

                    Assert.AreEqual(expected.Guid, result.Guid);
                    Assert.AreEqual(expected.Code, result.Code);
                    Assert.AreEqual(expected.Description, result.Description);
                    Assert.AreEqual(expected.EntityName, result.EntityName);
                    Assert.AreEqual(expected.EthosResources.FirstOrDefault(), result.EthosResources.FirstOrDefault());
                    Assert.AreEqual(expected.FieldHelp, result.FieldHelp);
                    Assert.AreEqual(expected.FieldName, result.FieldName);
                    Assert.AreEqual(expected.MultipleValues, result.MultipleValues);
                    Assert.AreEqual(expected.SourceTitle, result.SourceTitle);
                    Assert.AreEqual(expected.SourceValue, result.SourceValue);
                    Assert.AreEqual(expected.ValcodeTableName, result.ValcodeTableName);
                }
            }

            [TestMethod]
            public async Task GetsConfigurationSettingsByGuidNonCacheAsync()
            {
                foreach (var expected in _configurationSettingsCollection)
                {
                    var result = await configurationSettingsRepo.GetConfigurationSettingsByGuidAsync(expected.Guid, true);

                    Assert.AreEqual(expected.Guid, result.Guid);
                    Assert.AreEqual(expected.Code, result.Code);
                    Assert.AreEqual(expected.Description, result.Description);
                    Assert.AreEqual(expected.EntityName, result.EntityName);
                    Assert.AreEqual(expected.EthosResources.FirstOrDefault(), result.EthosResources.FirstOrDefault());
                    Assert.AreEqual(expected.FieldHelp, result.FieldHelp);
                    Assert.AreEqual(expected.FieldName, result.FieldName);
                    Assert.AreEqual(expected.MultipleValues, result.MultipleValues);
                    Assert.AreEqual(expected.SourceTitle, result.SourceTitle);
                    Assert.AreEqual(expected.SourceValue, result.SourceValue);
                    Assert.AreEqual(expected.ValcodeTableName, result.ValcodeTableName);
                }
            }

            [TestMethod]
            public async Task GetElfDuplicateCriteriaDictionary()
            {
                var result = await configurationSettingsRepo.GetAllDuplicateCriteriaAsync(false);

                Assert.IsInstanceOfType(result, typeof(Dictionary<string, string>));
                foreach (var dictItem in result)
                {
                    var elfDupl = elfDuplCriteria.FirstOrDefault(edc => edc.Recordkey == dictItem.Key);
                    Assert.AreEqual(elfDupl.Recordkey, dictItem.Key, "Key Value");
                    Assert.AreEqual(elfDupl.ElfduplDesc, dictItem.Value, "Value");
                }
            }

            [TestMethod]
            public async Task GetCashierDictionary()
            {
                var result = await configurationSettingsRepo.GetAllCashierNamesAsync(false);

                Assert.IsInstanceOfType(result, typeof(Dictionary<string, string>));
                foreach (var dictItem in result)
                {
                    var person = personCollection.FirstOrDefault(pc => pc.Recordkey == dictItem.Key);
                    Assert.AreEqual(person.Recordkey, dictItem.Key, "Key Value");
                    Assert.AreEqual(string.Concat(person.FirstName, " ", person.LastName), dictItem.Value, "Value");
                }
            }

            [TestMethod]
            public async Task GetRegUsersDictionary()
            {
                var result = await configurationSettingsRepo.GetAllRegUsersAsync(false);

                Assert.IsInstanceOfType(result, typeof(Dictionary<string, string>));
                Assert.AreEqual("WEBREG", result.FirstOrDefault().Key, "Key Value");
                Assert.AreEqual("WEBREG", result.FirstOrDefault().Value, "Value");
            }

            [TestMethod]
            public async Task GetValcodeDictionary()
            {
                var result = await configurationSettingsRepo.GetAllValcodeItemsAsync("CORE.VALCODES", "CONTACT.ROLES", false);

                Assert.IsInstanceOfType(result, typeof(Dictionary<string, string>));
                Assert.AreEqual("INTVR", result.FirstOrDefault().Key, "Key Value");
                Assert.AreEqual("Interviewer", result.FirstOrDefault().Value, "Value");
            }

            private ConfigurationSettingsRepository BuildValidConfigurationSettingsRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to ConfigurationSettings read
                var entityCollection = new Collection<IntgConfigSettings>(_configurationSettingsCollection.Select(record =>
                    new IntgConfigSettings()
                    {
                        Recordkey = record.Code,
                        RecordGuid = record.Guid,
                        IcsCollConfigDesc = record.FieldHelp,
                        IcsCollConfigTitle = record.Description,
                        IcsEthosResources = record.EthosResources,
                        IcsCollLdmFieldName = record.FieldName,
                        IcsCollEntity = record.EntityName,
                        IcsCollValcodeId = record.ValcodeTableName,
                        IcsMultipleValue = "N"
                    }).ToList());

                dataAccessorMock.Setup(ac => ac.SelectAsync("INTG.CONFIG.SETTINGS", ""))
                    .ReturnsAsync(new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11" });

                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<IntgConfigSettings>("INTG.CONFIG.SETTINGS", It.IsAny<string[]>(), true))
                    .ReturnsAsync(entityCollection);

                var ldmDefaults = new LdmDefaults()
                {
                    LdmdPersonDuplCriteria = "INTG.PERSON",
                    LdmdAddrDuplCriteria = "INTG.ADDRESS",
                    LdmdCheckFacultyLoad = "Y",
                    LdmdRegUsersId = "WEBREG",
                    LdmdCashier = "0003582",
                    LdmdCheckPostingPerformed = "N",
                    LdmdPrinInvestigatorRole = "INTVR",
                    LdmdMappingControl = "Update Ethos Value:Ethos",
                    LdmdProspectDuplCriteria = "INTG.PMR.PROSPECT",
                    LdmdRelationDuplCriteria = "INTG.PMR.RELATION",
                    LdmdVencontactDupCriteria = "INTG.PMR.VENCONTACT"
                };
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<LdmDefaults>("CORE.PARMS", "LDM.DEFAULTS", true)).ReturnsAsync(ldmDefaults);

                elfDuplCriteria = new Collection<ElfDuplCriteria>()
                {
                    new ElfDuplCriteria()
                    {
                        Recordkey = "INTG.PERSON",
                        ElfduplDesc = "Integration Person Matching"
                    },
                    new ElfDuplCriteria()
                    {
                        Recordkey = "INTG.ADDRESS",
                        ElfduplDesc = "Integration Address Matching"
                    },
                    new ElfDuplCriteria()
                    {
                        Recordkey = "INTG.PMR.PROSPECT",
                        ElfduplDesc = "Person Match Request Prospect"
                    },
                    new ElfDuplCriteria()
                    {
                        Recordkey = "INTG.PMR.RELATION",
                        ElfduplDesc = "Person Match Request Relation"
                    },
                    new ElfDuplCriteria()
                    {
                        Recordkey = "INTG.PMR.VENCONTACT",
                        ElfduplDesc = "Person Match Request Vendor Contact"
                    }
                };
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<ElfDuplCriteria>("ELF.DUPL.CRITERIA", "", true)).ReturnsAsync(elfDuplCriteria);

                var person = new DataContracts.Person()
                {
                    Recordkey = "0003582",
                    LastName = "Magnusson",
                    FirstName = "Steven"
                };
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<DataContracts.Person>("PERSON", It.IsAny<string>(), true)).ReturnsAsync(person);
                personCollection = new Collection<DataContracts.Person>() { person };
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.Person>("PERSON", It.IsAny<string[]>(), true)).ReturnsAsync(personCollection);

                ApplValcodes contactRolesResponse = new ApplValcodes
                {
                    ValsEntityAssociation = new List<ApplValcodesVals>
                    {
                        new ApplValcodesVals("INT", "Interviewer", "", "INTVR", "", "", "")
                    }
                };
                dataAccessorMock.Setup(ac => ac.SelectAsync("CORE.VALCODES", "CONTACT.ROLES"))
                     .ReturnsAsync(new string[] { "INTRV" });
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "CONTACT.ROLES", It.IsAny<bool>())).ReturnsAsync(contactRolesResponse);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>
                    {
                        {
                            string.Join("+", new string[] { "CORE.VALCODES", "CONTACT.ROLES", "INTVR" }),
                            new RecordKeyLookupResult() { Guid = "25d7f151-788d-4b32-a962-46ad1e3ab06c" }
                        }
                    };
                    return Task.FromResult(result);
                });

                dataAccessorMock.Setup(acc => acc.SelectAsync("REG.USERS", It.IsAny<string>())).ReturnsAsync(new string[] { "WEBREG" });

                // Construct repository
                var apiSettings = new ApiSettings("TEST");
                configurationSettingsRepo = new ConfigurationSettingsRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return configurationSettingsRepo;
            }
        }
    }
}
