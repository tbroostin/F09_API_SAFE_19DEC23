// Copyright 2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Transactions;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Data.Colleague;
using Ellucian.Web.Cache;
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
    public class EthosApiBuilderRepositoryTests
    {
        [TestClass]
        public class EthosApiBuildersGetMethods
        {
            Mock<ICacheProvider> iCacheProviderMock;
            Mock<IColleagueTransactionFactory> iColleagueTransactionFactoryMock;
            Mock<IColleagueTransactionInvoker> iColleagueTransactionInvokerMock;
            Mock<ILogger> iLoggerMock;
            Mock<IColleagueDataReader> dataReaderMock;

            EthosApiBuilderRepository ethosApiBuilderRepository;
            IEnumerable<EthosApiBuilder> ethosApiBuilderEntities;
            Dictionary<string, GuidLookupResult> guidLookupResults;
            EthosApiBuilder ethosApiBuilderEntity;
            FileSuiteTemplates faFileSuite;
            GetCacheApiKeysRequest request = new GetCacheApiKeysRequest();
            GetCacheApiKeysResponse response = new GetCacheApiKeysResponse();
            Collection<FileSuiteTemplates> cfFileSuites;
            Collection<EdmExtensions> edmExtensionsContracts;
            Collection<EdmExtVersions> edmExtVersionsContracts;
            Collection<EdmSelectCriteria> edmSelectCriteriaContracts;
            Collection<EdmQueries> edmQueriesContracts;
            Collection<EdmCodeHooks> edmCodeHooksContracts;
            Collection<EdmDepNotices> edmDepNoticesContracts;
            EthosApiConfiguration configuration;
            Dictionary<string, EthosExtensibleDataFilter> filterDictionary;
            Tuple<IEnumerable<EthosApiBuilder>, int> ethosApiBuilderTuple;
            string id = string.Empty;
            string recKey = string.Empty;
            int offset = 0;
            int limit = 50;
            bool bypassCache = false;

            [TestInitialize]
            public void Initialize()
            {
                iCacheProviderMock = new Mock<ICacheProvider>();
                iColleagueTransactionFactoryMock = new Mock<IColleagueTransactionFactory>();
                iLoggerMock = new Mock<ILogger>();
                dataReaderMock = new Mock<IColleagueDataReader>();
                iColleagueTransactionInvokerMock = new Mock<IColleagueTransactionInvoker>();
                iColleagueTransactionFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataReaderMock.Object);
                iColleagueTransactionFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(iColleagueTransactionInvokerMock.Object);

                BuildObjects();
                ethosApiBuilderRepository = new EthosApiBuilderRepository(iCacheProviderMock.Object, iColleagueTransactionFactoryMock.Object, iLoggerMock.Object);
            }

            private void BuildObjects()
            {
                id = "375ef15b-f2d2-40ed-ac47-f0d2d45260f0";
                recKey = "0012297";
                guidLookupResults = new Dictionary<string,GuidLookupResult>();
                guidLookupResults.Add("PERSON.HEALTH", new GuidLookupResult() { Entity = "PERSON.HEALTH", PrimaryKey = "0012297", SecondaryKey = "" });

                configuration = new EthosApiConfiguration()
                {
                    SelectFileName = "PERSON.HEALTH",
                    PrimaryGuidSource = "PERSON.HEALTH.ID",
                    PrimaryGuidDbType = "K",
                    PrimaryGuidFileName = "PERSON.HEALTH",
                    PrimaryTableName = "",
                    PrimaryEntity = "PERSON.HEALTH",
                    PageLimit = 50,
                    ResourceName = "person-health",
                    HttpMethods = new List<EthosApiSupportedMethods>()
                    {
                        new EthosApiSupportedMethods("GET", "")
                    },
                    SelectionCriteria = new List<EthosApiSelectCriteria>(),
                    SelectParagraph = new List<string>(),
                    SelectRules = new List<string>(),
                };

                filterDictionary = new Dictionary<string, EthosExtensibleDataFilter>();

                BuildData();
                BuildValidEthosApiBuilderRepository();
            }

            [TestCleanup]
            public void Cleanup()
            {
                ethosApiBuilderRepository = null;
                ethosApiBuilderEntities = null;
                guidLookupResults = null;
                ethosApiBuilderEntities = null;
                edmExtensionsContracts = null;
                edmExtVersionsContracts = null;
                edmSelectCriteriaContracts = null;
                edmQueriesContracts = null;
                edmDepNoticesContracts = null;
                configuration = null;
                filterDictionary = null;
                ethosApiBuilderTuple = null;
                request = null;
                response = null;
                id = string.Empty;
                recKey = string.Empty;
            }
            private EthosApiBuilder BuildEthosApiBuilder(EthosApiBuilder ethosApiBuilderEntity, Ellucian.Colleague.Data.Base.DataContracts.Person personContract)
            {
                EthosApiBuilder ethosApiBuilder = new EthosApiBuilder(id, recKey, "person-health");
                return ethosApiBuilder;
            }

            [TestMethod]
            public async Task EthosApiBuilderRepo_GetEthosApiBuilderAsync()
            {
                var resultTuple = await ethosApiBuilderRepository.GetEthosApiBuilderAsync(offset, limit, configuration, filterDictionary, bypassCache);
                var results = resultTuple.Item1;
                var total = resultTuple.Item2;

                Assert.AreEqual(3, total);
                var index = 0;
                foreach (var result in results)
                {
                    Assert.AreEqual(ethosApiBuilderEntities.ElementAt(index).Code, result.Code);
                    Assert.AreEqual(ethosApiBuilderEntities.ElementAt(index).Guid, result.Guid);
                    Assert.AreEqual(ethosApiBuilderEntities.ElementAt(index).Description, result.Description);
                    index++;
                }
            }


            [TestMethod]
            public async Task EthosApiBuilderRepo_GetEthosApiBuilderByIdAsync()
            {
                id = ethosApiBuilderEntities.FirstOrDefault().Guid;
                var result = await ethosApiBuilderRepository.GetEthosApiBuilderByIdAsync(id, configuration);

                Assert.AreEqual(ethosApiBuilderEntities.FirstOrDefault().Code, result.Code);
                Assert.AreEqual(ethosApiBuilderEntities.FirstOrDefault().Guid, result.Guid);
                Assert.AreEqual(ethosApiBuilderEntities.FirstOrDefault().Description, result.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task EthosApiBuildersRepo_GetEthosApiBuilderAsync_RepositoryException()
            {
                dataReaderMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(gla =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    return Task.FromResult(result);
                });

                try
                {
                    var resultTuple = await ethosApiBuilderRepository.GetEthosApiBuilderAsync(offset, limit, configuration, filterDictionary, bypassCache);
                }
                catch (RepositoryException ex)
                {
                    Assert.AreEqual(3, ex.Errors.Count);
                    Assert.AreEqual("Cannot find a guid for PERSON.HEALTH.", ex.Errors.ElementAt(1).Message);
                    Assert.AreEqual("GUID.Not.Found", ex.Errors.ElementAt(1).Code);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task EthosApiBuildersRepo_GetEthosApiBuilderByIdAsync_RepositoryException()
            {
                dataReaderMock.Setup(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
                {
                    var result = new Dictionary<string, GuidLookupResult>();
                    return Task.FromResult(result);
                });

                try
                {
                    var result = await ethosApiBuilderRepository.GetEthosApiBuilderByIdAsync(id, configuration);
                }
                catch (KeyNotFoundException ex)
                {
                    Assert.AreEqual("Invalid GUID for person-health: '375ef15b-f2d2-40ed-ac47-f0d2d45260f0'", ex.Message);
                    throw ex;
                }
            }

            private void BuildData()
            {
                edmExtensionsContracts = new Collection<EdmExtensions>() 
                {
                    new EdmExtensions()
                    {
                        Recordkey = "PERSON-HEALTH",
                    },
                    new EdmExtensions()
                    {
                        Recordkey = "X-COURSES",
                    },
                    new EdmExtensions()
                    {
                        Recordkey = "X-ADDR.TYPES",
                    },
                };

                edmExtVersionsContracts = new Collection<EdmExtVersions>()
                { 

                };

                edmQueriesContracts = new Collection<EdmQueries>()
                {

                };

                edmSelectCriteriaContracts = new Collection<EdmSelectCriteria>()
                {

                };

                edmCodeHooksContracts = new Collection<EdmCodeHooks>()
                {

                };

                edmDepNoticesContracts = new Collection<EdmDepNotices>()
                {

                };

                ethosApiBuilderEntities = new List<EthosApiBuilder>() 
                {
                    new EthosApiBuilder("8be66e85-85dc-447b-8a6a-e5edc7027dfa", "1", "PERSON.HEALTH"),
                    new EthosApiBuilder("c987c8bd-898d-4480-836b-64e424a3efe5", "2", "PERSON.HEALTH"),
                    new EthosApiBuilder("f90aefe5-8a33-497a-969a-641c6361e1e7", "3", "PERSON.HEALTH")
                };
                ethosApiBuilderEntity = ethosApiBuilderEntities.First();

                ethosApiBuilderTuple = new Tuple<IEnumerable<EthosApiBuilder>, int>(ethosApiBuilderEntities, ethosApiBuilderEntities.Count());

                faFileSuite = new FileSuiteTemplates()
                {
                    FstActiveFlag = new List<string>() { "Y" },
                    FstDescription = "Description",
                    FstFilePrefix = new List<string>() { "CS" },
                    FstTemplate = new List<string>() { "CS.ACYR" },
                    FstmpltEntityAssociation = new List<FileSuiteTemplatesFstmplt>()
                        {
                            new FileSuiteTemplatesFstmplt() { FstActiveFlagAssocMember = "Y", FstFilePrefixAssocMember = "CS", FstTemplateAssocMember = "CS.ACYR" }
                        }
                };

                cfFileSuites = new Collection<FileSuiteTemplates>()
                {
                    new FileSuiteTemplates()
                     {
                         FstActiveFlag = new List<string>() { "Y" },
                         FstDescription = "Description",
                         FstFilePrefix = new List<string>() { "GLA" },
                         FstTemplate = new List<string>() { "GLA.FYR" },
                         FstmpltEntityAssociation = new List<FileSuiteTemplatesFstmplt>()
                         {
                            new FileSuiteTemplatesFstmplt() { FstActiveFlagAssocMember = "Y", FstFilePrefixAssocMember = "GLA", FstTemplateAssocMember = "GLA.FYR" }
                         }
                     }
                };
            }

            public IEthosApiBuilderRepository BuildValidEthosApiBuilderRepository()
            {
                // Set up dataAccessorMock as the object for the DataAccessor
                iColleagueTransactionFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataReaderMock.Object);
                
                dataReaderMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(gla =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var gl in ethosApiBuilderEntities)
                    {
                        result.Add(gl.Code, new RecordKeyLookupResult() { Guid = gl.Guid });
                    }
                    return Task.FromResult(result);
                });

                dataReaderMock.Setup(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
                {
                    var result = new Dictionary<string, GuidLookupResult>();
                    foreach (var gl in ethosApiBuilderEntities)
                    {
                        result.Add(gl.Guid, new GuidLookupResult() { Entity = "PERSON.HEALTH", PrimaryKey = gl.Code });
                    }
                    return Task.FromResult(result);
                });
                dataReaderMock.Setup(acc => acc.SelectAsync("PERSON.HEALTH", new string[] { ethosApiBuilderEntities.FirstOrDefault().Code }, It.IsAny<string>())).ReturnsAsync(new string[] { ethosApiBuilderEntities.FirstOrDefault().Code });

                dataReaderMock.Setup(dr => dr.ReadRecordAsync<FileSuiteTemplates>("ST.PARMS", It.IsAny<string>(), true)).ReturnsAsync(faFileSuite);

                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<FileSuiteTemplates>("CF.PARMS", It.IsAny<string[]>(), true)).ReturnsAsync(cfFileSuites);

                iCacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                    x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                var resp = new GetCacheApiKeysResponse()
                {
                    Offset = 0,
                    Limit = 2,
                    CacheName = "AllEthosApiBuilders",
                    Entity = "PERSON.HEALTH",
                    Sublist = new List<string>() { "1", "2", "3" },
                    TotalCount = 3,
                    KeyCacheInfo = new List<KeyCacheInfo>()
                {
                    new KeyCacheInfo()
                    {
                        KeyCacheMax = 5905,
                        KeyCacheMin = 1,
                        KeyCachePart = "000",
                        KeyCacheSize = 5905
                    },
                    new KeyCacheInfo()
                    {
                        KeyCacheMax = 7625,
                        KeyCacheMin = 5906,
                        KeyCachePart = "001",
                        KeyCacheSize = 1720
                    }
                }
                };
                iColleagueTransactionInvokerMock.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                    .ReturnsAsync(resp);

                // Build  repository
                ethosApiBuilderRepository = new EthosApiBuilderRepository(iCacheProviderMock.Object, iColleagueTransactionFactoryMock.Object, iLoggerMock.Object);

                return ethosApiBuilderRepository;
            }
        }

        [TestClass]
        public class EthosApiBuildersPutMethods
        {
            Mock<ICacheProvider> iCacheProviderMock;
            Mock<IColleagueTransactionFactory> iColleagueTransactionFactoryMock;
            Mock<IColleagueTransactionInvoker> iColleagueTransactionInvokerMock;
            Mock<ILogger> iLoggerMock;
            Mock<IColleagueDataReader> dataReaderMock;

            EthosApiBuilderRepository ethosApiBuilderRepository;
            IEnumerable<EthosApiBuilder> ethosApiBuilderEntities;
            EthosApiBuilder ethosApiBuilderEntity;
            EthosApiConfiguration configuration;

            [TestInitialize]
            public void Initialize()
            {
                iCacheProviderMock = new Mock<ICacheProvider>();
                iColleagueTransactionFactoryMock = new Mock<IColleagueTransactionFactory>();
                iLoggerMock = new Mock<ILogger>();
                dataReaderMock = new Mock<IColleagueDataReader>();
                iColleagueTransactionInvokerMock = new Mock<IColleagueTransactionInvoker>();
                iColleagueTransactionFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataReaderMock.Object);
                iColleagueTransactionFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(iColleagueTransactionInvokerMock.Object);

                BuildObjects();
                ethosApiBuilderRepository = new EthosApiBuilderRepository(iCacheProviderMock.Object, iColleagueTransactionFactoryMock.Object, iLoggerMock.Object);
            }

            private void BuildObjects()
            {
                configuration = new EthosApiConfiguration()
                {
                    SelectFileName = "PERSON.HEALTH",
                    PrimaryGuidSource = "PERSON.HEALTH.ID",
                    PrimaryGuidDbType = "K",
                    PrimaryGuidFileName = "PERSON.HEALTH",
                    PrimaryTableName = "",
                    PrimaryEntity = "PERSON.HEALTH",
                    PageLimit = 50,
                    ResourceName = "person-health",
                    HttpMethods = new List<EthosApiSupportedMethods>()
                    {
                        new EthosApiSupportedMethods("GET", "")
                    },
                    SelectionCriteria = new List<EthosApiSelectCriteria>(),
                    SelectParagraph = new List<string>(),
                    SelectRules = new List<string>(),
                };
                
                BuildData();
                BuildValidEthosApiBuilderRepository();
            }

            [TestCleanup]
            public void Cleanup()
            {
                ethosApiBuilderRepository = null;
                ethosApiBuilderEntities = null;
                ethosApiBuilderEntities = null;
                configuration = null;
            }

            [TestMethod]
            public async Task EthosApiBuilderRepo_PutEthosApiBuilderAsync()
            {
                var updateResponse = new UpdateEthosApiBuilderResponse()
                {
                    Entity = "PERSON.HEALTH",
                    RecordGuid = ethosApiBuilderEntity.Guid,
                    RecordKey = ethosApiBuilderEntity.Code,
                    Errors = false,
                    UpdateEthosApiBuilderErrors = new List<UpdateEthosApiBuilderErrors>()
                };
                iColleagueTransactionInvokerMock.Setup(x => x.ExecuteAsync<UpdateEthosApiBuilderRequest, UpdateEthosApiBuilderResponse>(It.IsAny<UpdateEthosApiBuilderRequest>())).ReturnsAsync(updateResponse);

                var result = await ethosApiBuilderRepository.UpdateEthosApiBuilderAsync(ethosApiBuilderEntity, configuration);

                Assert.AreEqual(ethosApiBuilderEntity.Code, result.Code);
                Assert.AreEqual(ethosApiBuilderEntity.Guid, result.Guid);
                Assert.AreEqual(ethosApiBuilderEntity.Description, result.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task EthosApiBuildersRepo_PutEthosApiBuilderAsync_RepositoryException()
            {
                var updateResponse = new UpdateEthosApiBuilderResponse()
                {
                    Entity = "PERSON.HEALTH",
                    RecordGuid = ethosApiBuilderEntity.Guid,
                    RecordKey = ethosApiBuilderEntity.Code,
                    Errors = false,
                    UpdateEthosApiBuilderErrors = new List<UpdateEthosApiBuilderErrors>()
                    {
                        new UpdateEthosApiBuilderErrors() { ErrorCodes = "Validation.Exception", ErrorMessages = "Invalid Response/Request"}
                    }
                };
                iColleagueTransactionInvokerMock.Setup(x => x.ExecuteAsync<UpdateEthosApiBuilderRequest, UpdateEthosApiBuilderResponse>(It.IsAny<UpdateEthosApiBuilderRequest>())).ReturnsAsync(updateResponse);

                try
                {
                    var result = await ethosApiBuilderRepository.UpdateEthosApiBuilderAsync(ethosApiBuilderEntity, configuration);
                }
                catch (RepositoryException ex)
                {
                    Assert.AreEqual(1, ex.Errors.Count);
                    Assert.AreEqual("Invalid Response/Request", ex.Errors.ElementAt(0).Message);
                    Assert.AreEqual("Validation.Exception", ex.Errors.ElementAt(0).Code);
                    throw ex;
                }
            }

            private void BuildData()
            {
                ethosApiBuilderEntities = new List<EthosApiBuilder>()
                {
                    new EthosApiBuilder("8be66e85-85dc-447b-8a6a-e5edc7027dfa", "1", "PERSON.HEALTH"),
                    new EthosApiBuilder("c987c8bd-898d-4480-836b-64e424a3efe5", "2", "PERSON.HEALTH"),
                    new EthosApiBuilder("f90aefe5-8a33-497a-969a-641c6361e1e7", "3", "PERSON.HEALTH")
                };
                ethosApiBuilderEntity = ethosApiBuilderEntities.First();
            }

            public IEthosApiBuilderRepository BuildValidEthosApiBuilderRepository()
            {
                // Build  repository
                ethosApiBuilderRepository = new EthosApiBuilderRepository(iCacheProviderMock.Object, iColleagueTransactionFactoryMock.Object, iLoggerMock.Object);

                return ethosApiBuilderRepository;
            }
        }

        [TestClass]
        public class EthosApiBuildersDeleteMethods
        {
            Mock<ICacheProvider> iCacheProviderMock;
            Mock<IColleagueTransactionFactory> iColleagueTransactionFactoryMock;
            Mock<IColleagueTransactionInvoker> iColleagueTransactionInvokerMock;
            Mock<ILogger> iLoggerMock;
            Mock<IColleagueDataReader> dataReaderMock;

            EthosApiBuilderRepository ethosApiBuilderRepository;
            IEnumerable<EthosApiBuilder> ethosApiBuilderEntities;
            EthosApiBuilder ethosApiBuilderEntity;
            EthosApiConfiguration configuration;

            [TestInitialize]
            public void Initialize()
            {
                iCacheProviderMock = new Mock<ICacheProvider>();
                iColleagueTransactionFactoryMock = new Mock<IColleagueTransactionFactory>();
                iLoggerMock = new Mock<ILogger>();
                dataReaderMock = new Mock<IColleagueDataReader>();
                iColleagueTransactionInvokerMock = new Mock<IColleagueTransactionInvoker>();
                iColleagueTransactionFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataReaderMock.Object);
                iColleagueTransactionFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(iColleagueTransactionInvokerMock.Object);

                BuildObjects();
                ethosApiBuilderRepository = new EthosApiBuilderRepository(iCacheProviderMock.Object, iColleagueTransactionFactoryMock.Object, iLoggerMock.Object);
            }

            private void BuildObjects()
            {
                configuration = new EthosApiConfiguration()
                {
                    SelectFileName = "PERSON.HEALTH",
                    PrimaryGuidSource = "PERSON.HEALTH.ID",
                    PrimaryGuidDbType = "K",
                    PrimaryGuidFileName = "PERSON.HEALTH",
                    PrimaryTableName = "",
                    PrimaryEntity = "PERSON.HEALTH",
                    PageLimit = 50,
                    ResourceName = "person-health",
                    HttpMethods = new List<EthosApiSupportedMethods>()
                    {
                        new EthosApiSupportedMethods("GET", "")
                    },
                    SelectionCriteria = new List<EthosApiSelectCriteria>(),
                    SelectParagraph = new List<string>(),
                    SelectRules = new List<string>(),
                };

                BuildData();
                BuildValidEthosApiBuilderRepository();
            }

            [TestCleanup]
            public void Cleanup()
            {
                ethosApiBuilderRepository = null;
                ethosApiBuilderEntities = null;
                ethosApiBuilderEntities = null;
                configuration = null;
            }

            [TestMethod]
            public async Task EthosApiBuilderRepo_PutEthosApiBuilderAsync()
            {
                var deleteResponse = new DeleteEthosApiBuilderResponse()
                {
                    Errors = "",
                    DeleteEthosApiBuilderErrors = new List<DeleteEthosApiBuilderErrors>()
                };
                iColleagueTransactionInvokerMock.Setup(x => x.ExecuteAsync<DeleteEthosApiBuilderRequest, DeleteEthosApiBuilderResponse>(It.IsAny<DeleteEthosApiBuilderRequest>())).ReturnsAsync(deleteResponse);

                await ethosApiBuilderRepository.DeleteEthosApiBuilderAsync(ethosApiBuilderEntity.Guid, configuration);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task EthosApiBuildersRepo_PutEthosApiBuilderAsync_RepositoryException()
            {
                var deleteResponse = new DeleteEthosApiBuilderResponse()
                {
                    Errors = "1",
                    DeleteEthosApiBuilderErrors = new List<DeleteEthosApiBuilderErrors>()
                    {
                        new DeleteEthosApiBuilderErrors() { ErrorCodes = "Validation.Exception", ErrorMessages = "Invalid Response/Request"}
                    }
                };
                iColleagueTransactionInvokerMock.Setup(x => x.ExecuteAsync<DeleteEthosApiBuilderRequest, DeleteEthosApiBuilderResponse>(It.IsAny<DeleteEthosApiBuilderRequest>())).ReturnsAsync(deleteResponse);

                try
                {
                    await ethosApiBuilderRepository.DeleteEthosApiBuilderAsync(ethosApiBuilderEntity.Guid, configuration);
                }
                catch (RepositoryException ex)
                {
                    Assert.AreEqual(1, ex.Errors.Count);
                    Assert.AreEqual("Invalid Response/Request", ex.Errors.ElementAt(0).Message);
                    Assert.AreEqual("Validation.Exception", ex.Errors.ElementAt(0).Code);
                    Assert.AreEqual(ethosApiBuilderEntity.Guid, ex.Errors.ElementAt(0).Id);
                    throw ex;
                }
            }

            private void BuildData()
            {
                ethosApiBuilderEntities = new List<EthosApiBuilder>()
                {
                    new EthosApiBuilder("8be66e85-85dc-447b-8a6a-e5edc7027dfa", "1", "PERSON.HEALTH"),
                    new EthosApiBuilder("c987c8bd-898d-4480-836b-64e424a3efe5", "2", "PERSON.HEALTH"),
                    new EthosApiBuilder("f90aefe5-8a33-497a-969a-641c6361e1e7", "3", "PERSON.HEALTH")
                };
                ethosApiBuilderEntity = ethosApiBuilderEntities.First();
            }

            public IEthosApiBuilderRepository BuildValidEthosApiBuilderRepository()
            {
                // Build  repository
                ethosApiBuilderRepository = new EthosApiBuilderRepository(iCacheProviderMock.Object, iColleagueTransactionFactoryMock.Object, iLoggerMock.Object);

                return ethosApiBuilderRepository;
            }
        }
    }
}