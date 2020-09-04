//Copyright 2019-2020 Ellucian Company L.P. and its affiliates.  
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Dmi.Runtime;
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
    public class DefaultSettingsRepositoryTests
    {
        public static char _SM = Convert.ToChar(DynamicArray.SM);

        /// <summary>
        /// Test class for DefaultSettings codes
        /// </summary>
        [TestClass]
        public class DefaultSettingsTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<IColleagueTransactionInvoker> colleagueTransactionInvokerMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<DefaultSettings> _defaultSettingsCollection;
            DefaultSettings _defaultSetting;
            TestDefaultSettingsRepository _testDefaultSettingsRepository;
            string codeItemName;
            Collection<ArCodesBase> arCodes;
            Collection<ArTypesBase> arTypes;
            Collection<AcadLevelsBase> academicLevels;
            Collection<ApplicationStatusesBase> applicationStatuses;
            Collection<AsgmtContractTypesBase> assignmentContractTypes;
            Collection<BendedBase> bendedCodes;
            Collection<CredTypesBase> creditTypes;
            Collection<LoadPeriodsBase> loadPeriods;
            Collection<PaymentMethods> paymentMethods;
            Collection<PositionBase> positions;
            Collection<RcptTenderGlDistrBase> receiptTenderGlDistrCodes;
            Collection<DataContracts.Person> personCollection;

            DefaultSettingsRepository defaultSettingsRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build responses used for mocking
                _testDefaultSettingsRepository = new TestDefaultSettingsRepository();
                _defaultSettingsCollection = _testDefaultSettingsRepository.GetDefaultSettingsAsync(false);

                // Build repository
                defaultSettingsRepo = BuildValidDefaultSettingsRepository();
                codeItemName = defaultSettingsRepo.BuildFullCacheKey("AllIntgDefaultSettings");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                colleagueTransactionInvokerMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                _defaultSettingsCollection = null;
                defaultSettingsRepo = null;
            }

            [TestMethod]
            public async Task GetsDefaultSettingsCacheAsync()
            {
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(_defaultSettingsCollection, new SemaphoreSlim(1, 1)));

                var result = await defaultSettingsRepo.GetDefaultSettingsAsync(false);

                for (int i = 0; i < _defaultSettingsCollection.Count(); i++)
                {
                    Assert.AreEqual(_defaultSettingsCollection.ElementAt(i).Guid, result.ElementAt(i).Guid);
                    Assert.AreEqual(_defaultSettingsCollection.ElementAt(i).Code, result.ElementAt(i).Code);
                    Assert.AreEqual(_defaultSettingsCollection.ElementAt(i).Description, result.ElementAt(i).Description);
                    Assert.AreEqual(_defaultSettingsCollection.ElementAt(i).EntityName, result.ElementAt(i).EntityName);
                    Assert.AreEqual(_defaultSettingsCollection.ElementAt(i).EthosResources.FirstOrDefault(), result.ElementAt(i).EthosResources.FirstOrDefault());
                    Assert.AreEqual(_defaultSettingsCollection.ElementAt(i).FieldHelp, result.ElementAt(i).FieldHelp);
                    Assert.AreEqual(_defaultSettingsCollection.ElementAt(i).FieldName, result.ElementAt(i).FieldName);
                    Assert.AreEqual(_defaultSettingsCollection.ElementAt(i).SourceTitle, result.ElementAt(i).SourceTitle);
                    Assert.AreEqual(_defaultSettingsCollection.ElementAt(i).SourceValue, result.ElementAt(i).SourceValue);
                    Assert.AreEqual(_defaultSettingsCollection.ElementAt(i).ValcodeTableName, result.ElementAt(i).ValcodeTableName);
                }
            }

            [TestMethod]
            public async Task GetsDefaultSettingsNonCacheAsync()
            {
                var result = await defaultSettingsRepo.GetDefaultSettingsAsync(true);

                for (int i = 0; i < _defaultSettingsCollection.Count(); i++)
                {
                    Assert.AreEqual(_defaultSettingsCollection.ElementAt(i).Guid, result.ElementAt(i).Guid);
                    Assert.AreEqual(_defaultSettingsCollection.ElementAt(i).Code, result.ElementAt(i).Code);
                    Assert.AreEqual(_defaultSettingsCollection.ElementAt(i).Description, result.ElementAt(i).Description);
                    Assert.AreEqual(_defaultSettingsCollection.ElementAt(i).EntityName, result.ElementAt(i).EntityName);
                    int index = 0;
                    foreach (var resource in _defaultSettingsCollection.ElementAt(i).EthosResources)
                    {
                        Assert.AreEqual(resource.Resource, result.ElementAt(i).EthosResources.ElementAt(index).Resource);
                        Assert.AreEqual(resource.PropertyName, result.ElementAt(i).EthosResources.ElementAt(index).PropertyName);
                        index++;
                    }
                    Assert.AreEqual(_defaultSettingsCollection.ElementAt(i).FieldHelp, result.ElementAt(i).FieldHelp);
                    Assert.AreEqual(_defaultSettingsCollection.ElementAt(i).FieldName, result.ElementAt(i).FieldName);
                    Assert.AreEqual(_defaultSettingsCollection.ElementAt(i).SourceTitle, result.ElementAt(i).SourceTitle);
                    Assert.AreEqual(_defaultSettingsCollection.ElementAt(i).SourceValue, result.ElementAt(i).SourceValue);
                    Assert.AreEqual(_defaultSettingsCollection.ElementAt(i).ValcodeTableName, result.ElementAt(i).ValcodeTableName);
                }
            }

            [TestMethod]
            public async Task GetsDefaultSettingsByGuidCacheAsync()
            {
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(_defaultSettingsCollection, new SemaphoreSlim(1, 1)));

                foreach (var expected in _defaultSettingsCollection)
                {
                    var result = await defaultSettingsRepo.GetDefaultSettingsByGuidAsync(expected.Guid, false);

                    Assert.AreEqual(expected.Guid, result.Guid);
                    Assert.AreEqual(expected.Code, result.Code);
                    Assert.AreEqual(expected.Description, result.Description);
                    Assert.AreEqual(expected.EntityName, result.EntityName);
                    int index = 0;
                    foreach (var resource in expected.EthosResources)
                    {
                        Assert.AreEqual(resource.Resource, result.EthosResources.ElementAt(index).Resource);
                        Assert.AreEqual(resource.PropertyName, result.EthosResources.ElementAt(index).PropertyName);
                        index++;
                    }
                    Assert.AreEqual(expected.FieldHelp, result.FieldHelp);
                    Assert.AreEqual(expected.FieldName, result.FieldName);
                    Assert.AreEqual(expected.SourceTitle, result.SourceTitle);
                    Assert.AreEqual(expected.SourceValue, result.SourceValue);
                    Assert.AreEqual(expected.ValcodeTableName, result.ValcodeTableName);
                }
            }

            [TestMethod]
            public async Task GetsDefaultSettingsByGuidNonCacheAsync()
            {
                foreach (var expected in _defaultSettingsCollection)
                {
                    var result = await defaultSettingsRepo.GetDefaultSettingsByGuidAsync(expected.Guid, true);

                    Assert.AreEqual(expected.Guid, result.Guid);
                    Assert.AreEqual(expected.Code, result.Code);
                    Assert.AreEqual(expected.Description, result.Description);
                    Assert.AreEqual(expected.EntityName, result.EntityName);
                    int index = 0;
                    foreach (var resource in expected.EthosResources)
                    {
                        Assert.AreEqual(resource.Resource, result.EthosResources.ElementAt(index).Resource);
                        Assert.AreEqual(resource.PropertyName, result.EthosResources.ElementAt(index).PropertyName);
                        index++;
                    }
                    Assert.AreEqual(expected.FieldHelp, result.FieldHelp);
                    Assert.AreEqual(expected.FieldName, result.FieldName);
                    Assert.AreEqual(expected.SourceTitle, result.SourceTitle);
                    Assert.AreEqual(expected.SourceValue, result.SourceValue);
                    Assert.AreEqual(expected.ValcodeTableName, result.ValcodeTableName);
                }
            }

            [TestMethod]
            public async Task GetArCodesDictionary()
            {
                var result = await defaultSettingsRepo.GetAllArCodesAsync(false);

                Assert.IsInstanceOfType(result, typeof(Dictionary<string, string>));
                foreach (var dictItem in result)
                {
                    var arCode = arCodes.FirstOrDefault(edc => edc.Recordkey == dictItem.Key);
                    Assert.AreEqual(arCode.Recordkey, dictItem.Key, "Key Value");
                    Assert.AreEqual(arCode.ArcDesc, dictItem.Value, "Value");
                }
            }

            [TestMethod]
            public async Task GetArTypesDictionary()
            {
                var result = await defaultSettingsRepo.GetAllArTypesAsync(false);

                Assert.IsInstanceOfType(result, typeof(Dictionary<string, string>));
                foreach (var dictItem in result)
                {
                    var arType = arTypes.FirstOrDefault(edc => edc.Recordkey == dictItem.Key);
                    Assert.AreEqual(arType.Recordkey, dictItem.Key, "Key Value");
                    Assert.AreEqual(arType.ArtDesc, dictItem.Value, "Value");
                }
            }

            [TestMethod]
            public async Task GetAssignmentTypesDictionary()
            {
                var result = await defaultSettingsRepo.GetAllAssignmentContractTypesAsync(false);

                Assert.IsInstanceOfType(result, typeof(Dictionary<string, string>));
                foreach (var dictItem in result)
                {
                    var asgnmtType = assignmentContractTypes.FirstOrDefault(edc => edc.Recordkey == dictItem.Key);
                    Assert.AreEqual(asgnmtType.Recordkey, dictItem.Key, "Key Value");
                    Assert.AreEqual(asgnmtType.ActypDesc, dictItem.Value, "Value");
                }
            }

            [TestMethod]
            public async Task GetBendedCodesDictionary()
            {
                var result = await defaultSettingsRepo.GetAllBendedCodesAsync(false);

                Assert.IsInstanceOfType(result, typeof(Dictionary<string, string>));
                foreach (var dictItem in result)
                {
                    var bendedCode = bendedCodes.FirstOrDefault(edc => edc.Recordkey == dictItem.Key);
                    Assert.AreEqual(bendedCode.Recordkey, dictItem.Key, "Key Value");
                    Assert.AreEqual(bendedCode.BdDesc, dictItem.Value, "Value");
                }
            }

            [TestMethod]
            public async Task GetCreditTypesDictionary()
            {
                var result = await defaultSettingsRepo.GetAllCreditTypesAsync(false);

                Assert.IsInstanceOfType(result, typeof(Dictionary<string, string>));
                foreach (var dictItem in result)
                {
                    var credType = creditTypes.FirstOrDefault(edc => edc.Recordkey == dictItem.Key);
                    Assert.AreEqual(credType.Recordkey, dictItem.Key, "Key Value");
                    Assert.AreEqual(credType.CrtpDesc, dictItem.Value, "Value");
                }
            }

            [TestMethod]
            public async Task GetLoadPeriodsDictionary()
            {
                var result = await defaultSettingsRepo.GetAllLoadPeriodsAsync(false);

                Assert.IsInstanceOfType(result, typeof(Dictionary<string, string>));
                foreach (var dictItem in result)
                {
                    var loadPeriod = loadPeriods.FirstOrDefault(edc => edc.Recordkey == dictItem.Key);
                    Assert.AreEqual(loadPeriod.Recordkey, dictItem.Key, "Key Value");
                    Assert.AreEqual(loadPeriod.LdpdDesc, dictItem.Value, "Value");
                }
            }

            [TestMethod]
            public async Task GetPaymentMethodsDictionary()
            {
                var result = await defaultSettingsRepo.GetAllPaymentMethodsAsync(false);

                Assert.IsInstanceOfType(result, typeof(Dictionary<string, string>));
                foreach (var dictItem in result)
                {
                    var paymentMethod = paymentMethods.FirstOrDefault(edc => edc.Recordkey == dictItem.Key);
                    Assert.AreEqual(paymentMethod.Recordkey, dictItem.Key, "Key Value");
                    Assert.AreEqual(paymentMethod.PmthDescription, dictItem.Value, "Value");
                }
            }

            [TestMethod]
            public async Task GetPositionsDictionary()
            {
                var result = await defaultSettingsRepo.GetAllPositionsAsync(false);

                Assert.IsInstanceOfType(result, typeof(Dictionary<string, string>));
                foreach (var dictItem in result)
                {
                    var position = positions.FirstOrDefault(edc => edc.Recordkey == dictItem.Key);
                    Assert.AreEqual(position.Recordkey, dictItem.Key, "Key Value");
                    Assert.AreEqual(position.PosTitle, dictItem.Value, "Value");
                }
            }

            [TestMethod]
            public async Task GetRceiptTenderGlDistrDictionary()
            {
                var result = await defaultSettingsRepo.GetAllReceiptTenderGlDistrsAsync(false);

                Assert.IsInstanceOfType(result, typeof(Dictionary<string, string>));
                foreach (var dictItem in result)
                {
                    var glDistr = receiptTenderGlDistrCodes.FirstOrDefault(edc => edc.Recordkey == dictItem.Key);
                    Assert.AreEqual(glDistr.Recordkey, dictItem.Key, "Key Value");
                    Assert.AreEqual(glDistr.RcpttDesc, dictItem.Value, "Value");
                }
            }

            [TestMethod]
            public async Task GetAcaadLevelsDictionary()
            {
                var result = await defaultSettingsRepo.GetAllAcademicLevelsAsync(false);

                Assert.IsInstanceOfType(result, typeof(Dictionary<string, string>));
                foreach (var dictItem in result)
                {
                    var acadLevel = academicLevels.FirstOrDefault(edc => edc.Recordkey == dictItem.Key);
                    Assert.AreEqual(acadLevel.Recordkey, dictItem.Key, "Key Value");
                    Assert.AreEqual(acadLevel.AclvDesc, dictItem.Value, "Value");
                }
            }

            [TestMethod]
            public async Task GetApplicationStaffDictionary()
            {
                var result = await defaultSettingsRepo.GetAllApplicationStaffAsync(false);

                Assert.IsInstanceOfType(result, typeof(Dictionary<string, string>));
                foreach (var dictItem in result)
                {
                    var person = personCollection.FirstOrDefault(pc => pc.Recordkey == dictItem.Key);
                    Assert.AreEqual(person.Recordkey, dictItem.Key, "Key Value");
                    Assert.AreEqual(string.Concat(person.FirstName, " ", person.LastName).Trim(), dictItem.Value, "Value");
                }
            }

            [TestMethod]
            public async Task GetApprovalAgenciesDictionary()
            {
                var result = await defaultSettingsRepo.GetAllApprovalAgenciesAsync(false);

                Assert.IsInstanceOfType(result, typeof(Dictionary<string, string>));
                foreach (var dictItem in result)
                {
                    var person = personCollection.FirstOrDefault(pc => pc.Recordkey == dictItem.Key);
                    Assert.AreEqual(person.Recordkey, dictItem.Key, "Key Value");
                    if (person.PersonCorpIndicator == "Y")
                        Assert.AreEqual(person.PreferredName, dictItem.Value, "Value");
                    else
                        Assert.AreEqual(person.LastName, dictItem.Value, "Value");
                }
            }

            [TestMethod]
            public async Task GetSponsorsDictionary()
            {
                var result = await defaultSettingsRepo.GetAllSponsorsAsync(false);

                Assert.IsInstanceOfType(result, typeof(Dictionary<string, string>));
                foreach (var dictItem in result)
                {
                    var person = personCollection.FirstOrDefault(pc => pc.Recordkey == dictItem.Key);
                    Assert.AreEqual(person.Recordkey, dictItem.Key, "Key Value");
                    if (person.PersonCorpIndicator == "Y")
                        Assert.AreEqual(person.PreferredName, dictItem.Value, "Value");
                    else
                        Assert.AreEqual(person.LastName, dictItem.Value, "Value");
                }
            }

            [TestMethod]
            public async Task GetStaffApprovalsDictionary()
            {
                var result = await defaultSettingsRepo.GetAllStaffApprovalsAsync(false);

                Assert.IsInstanceOfType(result, typeof(Dictionary<string, string>));
                foreach (var dictItem in result)
                {
                    var person = personCollection.FirstOrDefault(pc => pc.Recordkey == dictItem.Key);
                    Assert.AreEqual(person.Recordkey, dictItem.Key, "Key Value");
                    Assert.AreEqual(string.Concat(person.FirstName, " ", person.LastName).Trim(), dictItem.Value, "Value");
                }
            }

            [TestMethod]
            public async Task GetApplicationStatusesDictionary()
            {
                var result = await defaultSettingsRepo.GetAllApplicationStatusesAsync(false);

                Assert.IsInstanceOfType(result, typeof(Dictionary<string, string>));
                Assert.AreEqual("PR", result.FirstOrDefault().Key, "Key Value");
                Assert.AreEqual("Prospect", result.FirstOrDefault().Value, "Value");
            }

            [TestMethod]
            public async Task GetPrivacyCodesValcodeDictionary()
            {
                var result = await defaultSettingsRepo.GetAllValcodeItemsAsync("CORE.VALCODES", "PRIVACY.CODES", false, "*");

                Assert.IsInstanceOfType(result, typeof(Dictionary<string, string>));
                Assert.AreEqual("G", result.FirstOrDefault().Key, "Key Value");
                Assert.AreEqual("Don't Release Grades", result.FirstOrDefault().Value, "Value");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task UpdateDefaultSettingsAsynceDictionary_ArgumentNullException()
            {
                var result = await defaultSettingsRepo.UpdateDefaultSettingsAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task UpdateDefaultSettingsAsynceDictionary()
            {
                var result = await defaultSettingsRepo.UpdateDefaultSettingsAsync(_defaultSetting);                
            }

            [TestMethod]
            public async Task GetDefaultSettingsAdvancedSearchOptionsAsync()
            {
                var result = await defaultSettingsRepo.GetDefaultSettingsAdvancedSearchOptionsAsync("Smith", "1", true);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task GetDefaultSettingsAdvancedSearchOptionsAsync_Errors()
            {
                var searchResponse = new SearchDefaultSettingsResponse()
                {

                    DefaultSettingAdvancedSearchOptionsErrors = new List<DefaultSettingAdvancedSearchOptionsErrors>()
                        {
                            new DefaultSettingAdvancedSearchOptionsErrors(){ ErrorCodes = "ErrorCode", ErrorMessages = "ErrorMessages"}
                        }
                };
                colleagueTransactionInvokerMock.Setup(i => i.ExecuteAsync<SearchDefaultSettingsRequest, SearchDefaultSettingsResponse>
                    (It.IsAny<SearchDefaultSettingsRequest>())).ReturnsAsync(searchResponse);

                var result = await defaultSettingsRepo.GetDefaultSettingsAdvancedSearchOptionsAsync("Smith", "1", true);
            }            

            private DefaultSettingsRepository BuildValidDefaultSettingsRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                colleagueTransactionInvokerMock = new Mock<IColleagueTransactionInvoker>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(colleagueTransactionInvokerMock.Object);

                // Setup response to DefaultSettings read
                var entityCollection = new Collection<IntgDefaultSettings>(_defaultSettingsCollection.Select(record =>
                    new IntgDefaultSettings()
                    {
                        Recordkey = record.Code,
                        RecordGuid = record.Guid,
                        IdsCollDefaultDesc = record.FieldHelp,
                        IdsCollDefaultTitle = record.Description,
                        IdsCollLdmFieldName = record.FieldName,
                        IdsCollEntity = record.EntityName,
                        IdsCollValcodeId = record.ValcodeTableName,
                        IdsEthosResources = record.EthosResources.Select(er => er.Resource).ToList(),
                        IdsEthosPropertyNames = record.EthosResources.Select(er => er.PropertyName).ToList(),
                        DefaultResourcesEntityAssociation = new List<IntgDefaultSettingsDefaultResources>(record.EthosResources.Select(er =>
                            new IntgDefaultSettingsDefaultResources()
                            {
                                IdsEthosResourcesAssocMember = er.Resource,
                                IdsEthosPropertyNamesAssocMember = er.PropertyName
                            }).ToList())
                    }).ToList());
                
                dataAccessorMock.Setup(ac => ac.SelectAsync("INTG.DEFAULT.SETTINGS", ""))
                    .ReturnsAsync(entityCollection.Select(ec => ec.Recordkey).ToArray());

                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<IntgDefaultSettings>("INTG.DEFAULT.SETTINGS", It.IsAny<string[]>(), true))
                    .ReturnsAsync(entityCollection);

                var ldmDefaults = new LdmDefaults()
                {
                    LdmdDefaultPrivacyCode = "G",
                    LdmdCourseActStatus = "A",
                    LdmdCourseInactStatus = "I",
                    LdmdDefaultApplStatStaff = "0003977",
                    LdmdDefaultApplStatus = "AC",
                    LdmdPrspctApplStatStaff = "0003977",
                    LdmdPrspctApplStatus = "PR",
                    LdmdDefaultArType = "01",
                    LdmdDefaultDistr = "BANK",
                    LdmdSponsorArCode = "111",
                    LdmdSponsorArType = "03",
                    LdmdDefaultSponsor = "0014122",
                    LdmdPaymentMethod = "ELEV",
                    LdmdDfltResLifeArType = "01",
                    LdmdBendedCode = new List<string>()
                    {
                        string.Concat("CAPL", _SM, "ARBL", _SM),
                        string.Concat(_SM, _SM),
                        string.Concat("CAMB", _SM, _SM)
                    }                    
                };
                ldmDefaults.LdmdChargeTypes = new List<string>() { "tuition", "fee", "housing", "meal" };
                ldmDefaults.LdmdDefaultArCodes = new List<string>() { "TUI", "ACTFE", "RESHL", "MEALS" };
                ldmDefaults.LdmdArDefaultsEntityAssociation = new List<LdmDefaultsLdmdArDefaults>()
                {
                    new LdmDefaultsLdmdArDefaults()
                    {
                        LdmdChargeTypesAssocMember = "tuition",
                        LdmdDefaultArCodesAssocMember = "TUI"
                    },
                    new LdmDefaultsLdmdArDefaults()
                    {
                        LdmdChargeTypesAssocMember = "fee",
                        LdmdDefaultArCodesAssocMember = "ACTFE"
                    },
                    new LdmDefaultsLdmdArDefaults()
                    {
                        LdmdChargeTypesAssocMember = "housing",
                        LdmdDefaultArCodesAssocMember = "RESHL"
                    },
                    new LdmDefaultsLdmdArDefaults()
                    {
                        LdmdChargeTypesAssocMember = "meal",
                        LdmdDefaultArCodesAssocMember = "MEALS"
                    }
                };

                ldmDefaults.LdmdCollDefaultsEntityAssociation = new List<LdmDefaultsLdmdCollDefaults>()
                {
                    new LdmDefaultsLdmdCollDefaults()
                    {
                        LdmdCollFieldNameAssocMember = "CRS.APPROVAL.AGENCY.IDS",
                        LdmdCollDefaultValueAssocMember = "0000043"
                    },
                    new LdmDefaultsLdmdCollDefaults()
                    {
                        LdmdCollFieldNameAssocMember = "CRS.APPROVAL.IDS",
                        LdmdCollDefaultValueAssocMember = "0003582"
                    },
                    new LdmDefaultsLdmdCollDefaults()
                    {
                        LdmdCollFieldNameAssocMember = "CRS.ACAD.LEVEL",
                        LdmdCollDefaultValueAssocMember = "UG"
                    },
                    new LdmDefaultsLdmdCollDefaults()
                    {
                        LdmdCollFieldNameAssocMember = "CRS.LEVELS",
                        LdmdCollDefaultValueAssocMember = "400"
                    },
                    new LdmDefaultsLdmdCollDefaults()
                    {
                        LdmdCollFieldNameAssocMember = "CSF.TEACHING.ARRANGEMENT",
                        LdmdCollDefaultValueAssocMember = "A"
                    },
                    new LdmDefaultsLdmdCollDefaults()
                    {
                        LdmdCollFieldNameAssocMember = "PAC.TYPE",
                        LdmdCollDefaultValueAssocMember = ""
                    },
                    new LdmDefaultsLdmdCollDefaults()
                    {
                        LdmdCollFieldNameAssocMember = "PLPP.POSITION.ID",
                        LdmdCollDefaultValueAssocMember = "0000072113"
                    },
                    new LdmDefaultsLdmdCollDefaults()
                    {
                        LdmdCollFieldNameAssocMember = "PLP.LOAD.PERIOD",
                        LdmdCollDefaultValueAssocMember = "19SP"
                    },
                    new LdmDefaultsLdmdCollDefaults()
                    {
                        LdmdCollFieldNameAssocMember = "CRS.APPROVAL.AGENCY.IDS",
                        LdmdCollDefaultValueAssocMember = "0000043"
                    },
                    new LdmDefaultsLdmdCollDefaults()
                    {
                        LdmdCollFieldNameAssocMember = "CRS.CRED.TYPE",
                        LdmdCollDefaultValueAssocMember = "CE"
                    },
                    new LdmDefaultsLdmdCollDefaults()
                    {
                        LdmdCollFieldNameAssocMember = "CRS.ACAD.LEVEL",
                        LdmdCollDefaultValueAssocMember = "UG"
                    }
                };
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<LdmDefaults>("CORE.PARMS", "LDM.DEFAULTS", true)).ReturnsAsync(ldmDefaults);

                arCodes = new Collection<ArCodesBase>();
                var dictArCodes = _testDefaultSettingsRepository.GetAllArCodesAsync(false);
                foreach (var dict in dictArCodes)
                {
                    arCodes.Add(new ArCodesBase() { Recordkey = dict.Key, ArcDesc = dict.Value });
                    dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ArCodesBase>(dict.Key, It.IsAny<bool>())).ReturnsAsync(arCodes.FirstOrDefault(al => al.Recordkey == dict.Key));
                }
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<ArCodesBase>("AR.CODES", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(arCodes);

                arTypes = new Collection<ArTypesBase>();
                var dictArTypes = _testDefaultSettingsRepository.GetAllArTypesAsync(false);
                foreach (var dict in dictArTypes)
                {
                    arTypes.Add(new ArTypesBase() { Recordkey = dict.Key, ArtDesc = dict.Value });
                    dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ArTypesBase>(dict.Key, It.IsAny<bool>())).ReturnsAsync(arTypes.FirstOrDefault(al => al.Recordkey == dict.Key));
                }
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<ArTypesBase>("AR.TYPES", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(arTypes);

                academicLevels = new Collection<AcadLevelsBase>();
                var dictAcademicLevels = _testDefaultSettingsRepository.GetAllAcademicLevelsAsync(false);
                foreach (var dict in dictAcademicLevels)
                {
                    academicLevels.Add(new AcadLevelsBase() { Recordkey = dict.Key, AclvDesc = dict.Value });
                    dataAccessorMock.Setup(acc => acc.ReadRecordAsync<AcadLevelsBase>(dict.Key, It.IsAny<bool>())).ReturnsAsync(academicLevels.FirstOrDefault(al => al.Recordkey == dict.Key));
                }
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<AcadLevelsBase>("ACAD.LEVELS", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(academicLevels);

                applicationStatuses = new Collection<ApplicationStatusesBase>();
                var dictApplicationStatuses = _testDefaultSettingsRepository.GetAllApplicationStatusesAsync(false);
                foreach (var dict in dictApplicationStatuses)
                {
                    applicationStatuses.Add(new ApplicationStatusesBase() { Recordkey = dict.Key, AppsDesc = dict.Value });
                    dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplicationStatusesBase>(dict.Key, It.IsAny<bool>())).ReturnsAsync(applicationStatuses.FirstOrDefault(al => al.Recordkey == dict.Key));
                }
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<ApplicationStatusesBase>("APPLICATION.STATUSES", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(applicationStatuses);

                assignmentContractTypes = new Collection<AsgmtContractTypesBase>();
                var dictAsgnContracts = _testDefaultSettingsRepository.GetAllAssignmentContractTypesAsync(false);
                foreach (var dict in dictAsgnContracts)
                {
                    assignmentContractTypes.Add(new AsgmtContractTypesBase() { Recordkey = dict.Key, ActypDesc = dict.Value });
                    dataAccessorMock.Setup(acc => acc.ReadRecordAsync<AsgmtContractTypesBase>(dict.Key, It.IsAny<bool>())).ReturnsAsync(assignmentContractTypes.FirstOrDefault(al => al.Recordkey == dict.Key));
                };
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<AsgmtContractTypesBase>("ASGMT.CONTRACT.TYPES", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(assignmentContractTypes);

                bendedCodes = new Collection<BendedBase>();
                var dictBended = _testDefaultSettingsRepository.GetAllBendedCodesAsync(false);
                foreach (var dict in dictBended)
                {
                    bendedCodes.Add(new BendedBase() { Recordkey = dict.Key, BdDesc = dict.Value });
                    dataAccessorMock.Setup(acc => acc.ReadRecordAsync<BendedBase>(dict.Key, It.IsAny<bool>())).ReturnsAsync(bendedCodes.FirstOrDefault(al => al.Recordkey == dict.Key));
                };
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<BendedBase>("BENDED", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(bendedCodes);


                creditTypes = new Collection<CredTypesBase>();
                var dictCredTypes = _testDefaultSettingsRepository.GetAllCreditTypesAsync(false);
                foreach (var dict in dictCredTypes)
                {
                    creditTypes.Add(new CredTypesBase() { Recordkey = dict.Key, CrtpDesc = dict.Value });
                    dataAccessorMock.Setup(acc => acc.ReadRecordAsync<CredTypesBase>(dict.Key, It.IsAny<bool>())).ReturnsAsync(creditTypes.FirstOrDefault(al => al.Recordkey == dict.Key));
                };
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<CredTypesBase>("CRED.TYPES", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(creditTypes);

                loadPeriods = new Collection<LoadPeriodsBase>();
                var dictLoadPeriods = _testDefaultSettingsRepository.GetAllLoadPeriodsAsync(false);
                foreach (var dict in dictLoadPeriods)
                {
                    loadPeriods.Add(new LoadPeriodsBase() { Recordkey = dict.Key, LdpdDesc = dict.Value });
                    dataAccessorMock.Setup(acc => acc.ReadRecordAsync<LoadPeriodsBase>(dict.Key, It.IsAny<bool>())).ReturnsAsync(loadPeriods.FirstOrDefault(al => al.Recordkey == dict.Key));
                };
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<LoadPeriodsBase>("LOAD.PERIODS", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(loadPeriods);

                paymentMethods = new Collection<PaymentMethods>();
                var dictPaymentMethods = _testDefaultSettingsRepository.GetAllPaymentMethodsAsync(false);
                foreach (var dict in dictPaymentMethods)
                {
                    paymentMethods.Add(new PaymentMethods() { Recordkey = dict.Key, PmthDescription = dict.Value });
                    dataAccessorMock.Setup(acc => acc.ReadRecordAsync<PaymentMethods>(dict.Key, It.IsAny<bool>())).ReturnsAsync(paymentMethods.FirstOrDefault(al => al.Recordkey == dict.Key));
                };
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<PaymentMethods>("PAYMENT.METHODS", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(paymentMethods);

                positions = new Collection<PositionBase>();
                var dictPositions = _testDefaultSettingsRepository.GetAllPositionsAsync(false);
                foreach (var dict in dictPositions)
                {
                    positions.Add(new PositionBase() { Recordkey = dict.Key, PosTitle = dict.Value });
                    dataAccessorMock.Setup(acc => acc.ReadRecordAsync<PositionBase>(dict.Key, It.IsAny<bool>())).ReturnsAsync(positions.FirstOrDefault(al => al.Recordkey == dict.Key));
                };
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<PositionBase>("POSITION", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(positions);

                receiptTenderGlDistrCodes = new Collection<RcptTenderGlDistrBase>();
                var dictDistr = _testDefaultSettingsRepository.GetAllReceiptTenderGlDistrsAsync(false);
                foreach (var dict in dictDistr)
                {
                    receiptTenderGlDistrCodes.Add(new RcptTenderGlDistrBase() { Recordkey = dict.Key, RcpttDesc = dict.Value });
                    dataAccessorMock.Setup(acc => acc.ReadRecordAsync<RcptTenderGlDistrBase>(dict.Key, It.IsAny<bool>())).ReturnsAsync(receiptTenderGlDistrCodes.FirstOrDefault(al => al.Recordkey == dict.Key));
                };
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<RcptTenderGlDistrBase>("RCPT.TENDER.GL.DISTR", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(receiptTenderGlDistrCodes);

                personCollection = new Collection<DataContracts.Person>();
                var approvalAgency = new DataContracts.Person()
                {
                    Recordkey = "0000043",
                    LastName = "ELLUCIAN UNIVERSITY",
                    PreferredName = "Ellucian University 2nd Line Of Name",
                    PersonCorpIndicator = "Y"
                };
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<DataContracts.Person>("PERSON", approvalAgency.Recordkey, true)).ReturnsAsync(approvalAgency);
                personCollection.Add(approvalAgency);

                var approvalPerson = new DataContracts.Person()
                {
                    Recordkey = "0003582",
                    LastName = "Magnusson",
                    FirstName = "Steven",
                    PersonCorpIndicator = "N"
                };
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<DataContracts.Person>("PERSON", approvalPerson.Recordkey, true)).ReturnsAsync(approvalPerson);
                personCollection.Add(approvalPerson);

                var applStatusPerson = new DataContracts.Person()
                {
                    Recordkey = "0003977",
                    LastName = "Alvano",
                    FirstName = "Bernard",
                    PersonCorpIndicator = "N"
                };
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<DataContracts.Person>("PERSON", applStatusPerson.Recordkey, true)).ReturnsAsync(applStatusPerson);
                personCollection.Add(applStatusPerson);

                var sponsorPerson = new DataContracts.Person()
                {
                    Recordkey = "0014122",
                    LastName = "Nyman",
                    FirstName = "Kelly",
                    PersonCorpIndicator = "N"
                };
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<DataContracts.Person>("PERSON", sponsorPerson.Recordkey, true)).ReturnsAsync(sponsorPerson);
                personCollection.Add(sponsorPerson);

                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.Person>("PERSON", It.IsAny<string[]>(), true)).ReturnsAsync(personCollection);

                ApplValcodes privacyCodesResponse = new ApplValcodes
                {
                    ValsEntityAssociation = new List<ApplValcodesVals>
                    {
                        new ApplValcodesVals("G", "Don't Release Grades", "", "G", "", "", "")
                    }
                };
                dataAccessorMock.Setup(ac => ac.SelectAsync("CORE.VALCODES", "PRIVACY.CODES"))
                     .ReturnsAsync(new string[] { "G" });
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "PRIVACY.CODES", It.IsAny<bool>())).ReturnsAsync(privacyCodesResponse);

                ApplValcodes courseLevelsResponse = new ApplValcodes
                {
                    ValsEntityAssociation = new List<ApplValcodesVals>
                    {
                        new ApplValcodesVals("400", "Fourth Year", "", "400", "", "", "")
                    }
                };
                dataAccessorMock.Setup(ac => ac.SelectAsync("ST.VALCODES", "COURSE.LEVELS"))
                     .ReturnsAsync(new string[] { "400" });
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "COURSE.LEVELS", It.IsAny<bool>())).ReturnsAsync(courseLevelsResponse);
                
                ApplValcodes courseStatusesResponse = new ApplValcodes
                {
                    ValsEntityAssociation = new List<ApplValcodesVals>
                    {
                        new ApplValcodesVals("A", "Active", "", "A", "", "", ""),
                        new ApplValcodesVals("I", "Inactive", "", "I", "", "", "")
                    }
                };
                dataAccessorMock.Setup(ac => ac.SelectAsync("ST.VALCODES", "COURSE.STATUSES"))
                     .ReturnsAsync(new string[] { "I", "A" });
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "COURSE.STATUSES", It.IsAny<bool>())).ReturnsAsync(courseStatusesResponse);

                ApplValcodes teachingArrangementsResponse = new ApplValcodes
                {
                    ValsEntityAssociation = new List<ApplValcodesVals>
                    {
                        new ApplValcodesVals("A", "Instructors alternate", "", "A", "", "", "")
                    }
                };
                dataAccessorMock.Setup(ac => ac.SelectAsync("ST.VALCODES", "TEACHING.ARRANGEMENTS"))
                     .ReturnsAsync(new string[] { "A" });
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "TEACHING.ARRANGEMENTS", It.IsAny<bool>())).ReturnsAsync(teachingArrangementsResponse);
                
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                _defaultSetting = _testDefaultSettingsRepository.GetDefaultSettingsAsync(false).First();
                var updateRequest = new UpdateDefaultSettingsRequest()
                {
                    Guid = _defaultSetting.Guid
                };
                var updateResponse = new UpdateDefaultSettingsResponse()
                {
                    Guid = _defaultSetting.Guid,
                    DefaultSettingErrors = new List<DefaultSettingErrors>()
                    {
                        new DefaultSettingErrors(){ ErrorCodes = "ErrorCode", ErrorMessages = "ErrorMessages"}
                    }
                };
                Dictionary<string, GuidLookupResult> defSettDict = new Dictionary<string, GuidLookupResult>();
                defSettDict.Add("1", new GuidLookupResult() { Entity = "INTG.DEFAULT.SETTINGS", PrimaryKey = "1" });
                dataAccessorMock.Setup(x => x.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(defSettDict);
                colleagueTransactionInvokerMock.Setup(i => i.ExecuteAsync<UpdateDefaultSettingsRequest, UpdateDefaultSettingsResponse>(It.IsAny< UpdateDefaultSettingsRequest>())).ReturnsAsync(updateResponse);
                      
                var searchResponse = new SearchDefaultSettingsResponse()
                {
                    MatchingData = new List<MatchingData>()
                    {
                        new MatchingData(){Titles= "Professor", Values = "123", Origins = "POSITION"}
                    }
                };
                colleagueTransactionInvokerMock.Setup(i => i.ExecuteAsync<SearchDefaultSettingsRequest, SearchDefaultSettingsResponse>
                (It.IsAny<SearchDefaultSettingsRequest>())).ReturnsAsync(searchResponse);
                               
                // Construct repository
                var apiSettings = new ApiSettings("TEST");
                defaultSettingsRepo = new DefaultSettingsRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return defaultSettingsRepo;
            }
        }
    }
}
