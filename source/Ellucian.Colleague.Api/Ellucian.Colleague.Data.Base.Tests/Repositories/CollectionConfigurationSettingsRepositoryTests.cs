//Copyright 2020-2023 Ellucian Company L.P. and its affiliates.  
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Domain.Exceptions;
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
    public class CollectionConfigurationSettingsRepositoryTests
    {
        /// <summary>
        /// Test class for CollectionConfigurationSettings codes
        /// </summary>
        [TestClass]
        public class CollectionConfigurationSettingsTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<CollectionConfigurationSettings> _collectionConfigurationSettingsCollection;
            TestCollectionConfigurationSettingsRepository _testCollectionConfigurationSettingsRepository;
            string fullCacheKeyName;
            Collection<RelationTypes> relationTypeCodes;
            Collection<BendedBase> bendedCodes;

            CollectionConfigurationSettingsRepository collectionConfigurationSettingsRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build responses used for mocking
                _testCollectionConfigurationSettingsRepository = new TestCollectionConfigurationSettingsRepository();
                _collectionConfigurationSettingsCollection = _testCollectionConfigurationSettingsRepository.GetCollectionConfigurationSettingsAsync(false);

                // Build repository
                collectionConfigurationSettingsRepo = BuildValidCollectionConfigurationSettingsRepository();
                fullCacheKeyName = collectionConfigurationSettingsRepo.BuildFullCacheKey("AllIntgCollectConfigSettings");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                _collectionConfigurationSettingsCollection = null;
                collectionConfigurationSettingsRepo = null;
            }

            [TestMethod]
            public async Task GetsCollectionConfigurationSettingsCacheAsync()
            {
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(_collectionConfigurationSettingsCollection, new SemaphoreSlim(1, 1)));

                var result = await collectionConfigurationSettingsRepo.GetCollectionConfigurationSettingsAsync(false);

                for (int i = 0; i < _collectionConfigurationSettingsCollection.Count(); i++)
                {
                    Assert.AreEqual(_collectionConfigurationSettingsCollection.ElementAt(i).Guid, result.ElementAt(i).Guid);
                    Assert.AreEqual(_collectionConfigurationSettingsCollection.ElementAt(i).Code, result.ElementAt(i).Code);
                    Assert.AreEqual(_collectionConfigurationSettingsCollection.ElementAt(i).Description, result.ElementAt(i).Description);
                    Assert.AreEqual(_collectionConfigurationSettingsCollection.ElementAt(i).EntityName, result.ElementAt(i).EntityName);
                    Assert.AreEqual(_collectionConfigurationSettingsCollection.ElementAt(i).EthosResources.FirstOrDefault(), result.ElementAt(i).EthosResources.FirstOrDefault());
                    Assert.AreEqual(_collectionConfigurationSettingsCollection.ElementAt(i).FieldHelp, result.ElementAt(i).FieldHelp);
                    Assert.AreEqual(_collectionConfigurationSettingsCollection.ElementAt(i).FieldName, result.ElementAt(i).FieldName);
                    var x = 0;
                    foreach (var source in _collectionConfigurationSettingsCollection.ElementAt(i).Source)
                    {
                        Assert.AreEqual(source.SourceTitle, result.ElementAt(i).Source.ElementAt(x).SourceTitle);
                        Assert.AreEqual(source.SourceValue, result.ElementAt(i).Source.ElementAt(x).SourceValue);
                        x++;
                    }
                    Assert.AreEqual(_collectionConfigurationSettingsCollection.ElementAt(i).ValcodeTableName, result.ElementAt(i).ValcodeTableName);
                }
            }

            [TestMethod]
            public async Task GetsCollectionConfigurationSettingsNonCacheAsync()
            {
                var result = await collectionConfigurationSettingsRepo.GetCollectionConfigurationSettingsAsync(true);

                for (int i = 0; i < _collectionConfigurationSettingsCollection.Count(); i++)
                {
                    Assert.AreEqual(_collectionConfigurationSettingsCollection.ElementAt(i).Guid, result.ElementAt(i).Guid);
                    Assert.AreEqual(_collectionConfigurationSettingsCollection.ElementAt(i).Code, result.ElementAt(i).Code);
                    Assert.AreEqual(_collectionConfigurationSettingsCollection.ElementAt(i).Description, result.ElementAt(i).Description);
                    Assert.AreEqual(_collectionConfigurationSettingsCollection.ElementAt(i).EntityName, result.ElementAt(i).EntityName);
                    int index = 0;
                    foreach (var resource in _collectionConfigurationSettingsCollection.ElementAt(i).EthosResources)
                    {
                        Assert.AreEqual(resource.Resource, result.ElementAt(i).EthosResources.ElementAt(index).Resource);
                        Assert.AreEqual(resource.PropertyName, result.ElementAt(i).EthosResources.ElementAt(index).PropertyName);
                        index++;
                    }
                    Assert.AreEqual(_collectionConfigurationSettingsCollection.ElementAt(i).FieldHelp, result.ElementAt(i).FieldHelp);
                    Assert.AreEqual(_collectionConfigurationSettingsCollection.ElementAt(i).FieldName, result.ElementAt(i).FieldName);
                    var x = 0;
                    foreach (var source in _collectionConfigurationSettingsCollection.ElementAt(i).Source)
                    {
                        Assert.AreEqual(source.SourceTitle, result.ElementAt(i).Source.ElementAt(x).SourceTitle);
                        Assert.AreEqual(source.SourceValue, result.ElementAt(i).Source.ElementAt(x).SourceValue);
                        x++;
                    }
                    Assert.AreEqual(_collectionConfigurationSettingsCollection.ElementAt(i).ValcodeTableName, result.ElementAt(i).ValcodeTableName);
                }
            }

            [TestMethod]
            public async Task GetsCollectionConfigurationSettingsByGuidCacheAsync()
            {
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(_collectionConfigurationSettingsCollection, new SemaphoreSlim(1, 1)));

                foreach (var expected in _collectionConfigurationSettingsCollection)
                {
                    var result = await collectionConfigurationSettingsRepo.GetCollectionConfigurationSettingsByGuidAsync(expected.Guid, false);

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
                    var x = 0;
                    foreach (var source in expected.Source)
                    {
                        Assert.AreEqual(source.SourceTitle, result.Source.ElementAt(x).SourceTitle);
                        Assert.AreEqual(source.SourceValue, result.Source.ElementAt(x).SourceValue);
                        x++;
                    }
                    Assert.AreEqual(expected.ValcodeTableName, result.ValcodeTableName);
                }
            }

            [TestMethod]
            public async Task GetsCollectionConfigurationSettingsByGuidNonCacheAsync()
            {
                foreach (var expected in _collectionConfigurationSettingsCollection)
                {
                    var result = await collectionConfigurationSettingsRepo.GetCollectionConfigurationSettingsByGuidAsync(expected.Guid, true);

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
                    var x = 0;
                    foreach (var source in expected.Source)
                    {
                        Assert.AreEqual(source.SourceTitle, result.Source.ElementAt(x).SourceTitle);
                        Assert.AreEqual(source.SourceValue, result.Source.ElementAt(x).SourceValue);
                        x++;
                    }
                    Assert.AreEqual(expected.ValcodeTableName, result.ValcodeTableName);
                }
            }

            [TestMethod]
            public async Task GetRelationTypesDictionary()
            {
                var result = await collectionConfigurationSettingsRepo.GetAllRelationTypesCodesAsync(false);

                Assert.IsInstanceOfType(result, typeof(Dictionary<string, string>));
                foreach (var dictItem in result)
                {
                    var relationType = relationTypeCodes.FirstOrDefault(edc => edc.Recordkey == dictItem.Key);
                    Assert.AreEqual(relationType.Recordkey, dictItem.Key, "Key Value");
                    Assert.AreEqual(relationType.ReltyDesc, dictItem.Value, "Value");
                }
            }

            [TestMethod]
            public async Task GetBendedCodesDictionary()
            {
                var result = await collectionConfigurationSettingsRepo.GetAllBendedCodesAsync(false);

                Assert.IsInstanceOfType(result, typeof(Dictionary<string, string>));
                foreach (var dictItem in result)
                {
                    var bendedCode = bendedCodes.FirstOrDefault(edc => edc.Recordkey == dictItem.Key);
                    Assert.AreEqual(bendedCode.Recordkey, dictItem.Key, "Key Value");
                    Assert.AreEqual(bendedCode.BdDesc, dictItem.Value, "Value");
                }
            }

            [TestMethod]
            public async Task GetBendedCodesDictionary_ExceptionReturnEmptyCollection()
            {

                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<BendedBase>("BENDED", It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new RepositoryException());

                var result = await collectionConfigurationSettingsRepo.GetAllBendedCodesAsync(false);

                Assert.IsInstanceOfType(result, typeof(Dictionary<string, string>));
                Assert.IsTrue(result.Count == 0);
            }

            [TestMethod]
            public async Task GetSectionRegistrationStatusesDictionary()
            {
                var result = await collectionConfigurationSettingsRepo.GetAllValcodeItemsAsync("ST.VALCODES", "STUDENT.ACAD.CRED.STATUSES", false);

                Assert.IsInstanceOfType(result, typeof(Dictionary<string, string>));
                Assert.AreEqual("N", result.FirstOrDefault().Key, "Key Value");
                Assert.AreEqual("New", result.FirstOrDefault().Value, "Value");
            }

            private CollectionConfigurationSettingsRepository BuildValidCollectionConfigurationSettingsRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to CollectionConfigurationSettings read
                var entityCollection = new Collection<IntgCollectSettings>(_collectionConfigurationSettingsCollection.Select(record =>
                    new IntgCollectSettings()
                    {
                        Recordkey = record.Code,
                        RecordGuid = record.Guid,
                        IclCollDefaultDesc = record.FieldHelp,
                        IclCollDefaultTitle = record.Description,
                        IclCollLdmFieldName = record.FieldName,
                        IclCollEntity = record.EntityName,
                        IclCollValcodeId = record.ValcodeTableName,
                        IclEthosResources = record.EthosResources.Select(er => er.Resource).ToList(),
                        IclEthosPropertyNames = record.EthosResources.Select(er => er.PropertyName).ToList(),
                        CollectResourcesEntityAssociation = new List<IntgCollectSettingsCollectResources>(record.EthosResources.Select(er =>
                            new IntgCollectSettingsCollectResources()
                            {
                                IclEthosResourcesAssocMember = er.Resource,
                                IclEthosPropertyNamesAssocMember = er.PropertyName
                            }).ToList())
                    }).ToList());

                dataAccessorMock.Setup(ac => ac.SelectAsync("INTG.COLLECT.SETTINGS", ""))
                    .ReturnsAsync(entityCollection.Select(ec => ec.Recordkey).ToArray());

                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<IntgCollectSettings>("INTG.COLLECT.SETTINGS", It.IsAny<string[]>(), true))
                    .ReturnsAsync(entityCollection);

                var ldmDefaults = new LdmDefaults()
                {
                    LdmdIncludeEnrlHeadcounts = new List<string>() { "N", "A", "D", "W" },
                    LdmdDfltAdmOfficeCodes = new List<string>() { "ADM", "AM" },
                    LdmdExcludeBenefits = new List<string>() { "4JAN", "STAN" },
                    LdmdLeaveStatusCodes = new List<string>() { "PR", "TE", "SW" },
                    LdmdGuardianRelTypes = new List<string>() { "GU", "P" }
                };
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<LdmDefaults>("CORE.PARMS", "LDM.DEFAULTS", true)).ReturnsAsync(ldmDefaults);

                relationTypeCodes = new Collection<RelationTypes>();
                var dictRelationTypes = _testCollectionConfigurationSettingsRepository.GetAllRelationTypesCodesAsync(false);
                foreach (var dict in dictRelationTypes)
                {
                    relationTypeCodes.Add(new RelationTypes() { Recordkey = dict.Key, ReltyDesc = dict.Value });
                    dataAccessorMock.Setup(acc => acc.ReadRecordAsync<RelationTypes>(dict.Key, It.IsAny<bool>())).ReturnsAsync(relationTypeCodes.FirstOrDefault(al => al.Recordkey == dict.Key));
                }
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<RelationTypes>("RELATION.TYPES", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(relationTypeCodes);

                bendedCodes = new Collection<BendedBase>();
                var dictBended = _testCollectionConfigurationSettingsRepository.GetAllBendedCodesAsync(false);
                foreach (var dict in dictBended)
                {
                    bendedCodes.Add(new BendedBase() { Recordkey = dict.Key, BdDesc = dict.Value });
                    dataAccessorMock.Setup(acc => acc.ReadRecordAsync<BendedBase>(dict.Key, It.IsAny<bool>())).ReturnsAsync(bendedCodes.FirstOrDefault(al => al.Recordkey == dict.Key));
                };
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<BendedBase>("BENDED", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(bendedCodes);

                ApplValcodes studentAcadCredStatusesResponse = new ApplValcodes();
                studentAcadCredStatusesResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                var studentAcadCredStatusDict = _testCollectionConfigurationSettingsRepository.GetAllValcodeItemsAsync("ST.VALCODES", "STUDENT.ACAD.CRED.STATUSES", true);
                foreach (var dict in studentAcadCredStatusDict)
                {
                    var valcodeEntry = new ApplValcodesVals(dict.Key, dict.Value, "", dict.Key, "", "", "");
                    studentAcadCredStatusesResponse.ValsEntityAssociation.Add(valcodeEntry);
                }
                dataAccessorMock.Setup(ac => ac.SelectAsync("ST.VALCODES", "STUDENT.ACAD.CRED.STATUSES"))
                     .ReturnsAsync(new string[] { studentAcadCredStatusDict.ElementAt(0).Key });
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "STUDENT.ACAD.CRED.STATUSES", It.IsAny<bool>())).ReturnsAsync(studentAcadCredStatusesResponse);

                ApplValcodes officeCodesResponse = new ApplValcodes();
                officeCodesResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                var officeCodesDict = _testCollectionConfigurationSettingsRepository.GetAllValcodeItemsAsync("CORE.VALCODES", "OFFICE.CODES", true);
                foreach (var dict in officeCodesDict)
                {
                    var valcodeEntry = new ApplValcodesVals(dict.Key, dict.Value, "", dict.Key, "", "", "");
                    officeCodesResponse.ValsEntityAssociation.Add(valcodeEntry);
                }
                dataAccessorMock.Setup(ac => ac.SelectAsync("CORE.VALCODES", "OFFICE.CODES"))
                     .ReturnsAsync(new string[] { officeCodesDict.ElementAt(0).Key });
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "OFFICE.CODES", It.IsAny<bool>())).ReturnsAsync(officeCodesResponse);

                ApplValcodes hrStatusesResponse = new ApplValcodes();
                hrStatusesResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                var hrStatusesDict = _testCollectionConfigurationSettingsRepository.GetAllValcodeItemsAsync("HR.VALCODES", "HR.STATUSES", false);
                foreach (var dict in hrStatusesDict)
                {
                    var valcodeEntry = new ApplValcodesVals(dict.Key, dict.Value, "", dict.Key, "", "", "");
                    hrStatusesResponse.ValsEntityAssociation.Add(valcodeEntry);
                }
                dataAccessorMock.Setup(ac => ac.SelectAsync("HR.VALCODES", "HR.STATUSES"))
                     .ReturnsAsync(new string[] { hrStatusesDict.ElementAt(0).Key });
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("HR.VALCODES", "HR.STATUSES", It.IsAny<bool>())).ReturnsAsync(hrStatusesResponse);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // Construct repository
                var apiSettings = new ApiSettings("TEST");
                collectionConfigurationSettingsRepo = new CollectionConfigurationSettingsRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return collectionConfigurationSettingsRepo;
            }
        }
    }
}
