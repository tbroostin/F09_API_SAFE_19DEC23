//Copyright 2015-2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Data.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using GlAccts = Ellucian.Colleague.Data.ColleagueFinance.DataContracts.GlAccts;
using Projects = Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Projects;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Exceptions;

namespace Ellucian.Colleague.Data.ColleagueFinance.Tests.Repositories
{
    [TestClass]
    public class ColleagueFinanceReferenceDataRepositoryTests : BaseRepositorySetup
    {
        #region Initialize and Cleanup

        private Mock<BaseColleagueRepository> repositoryMock = null;
        private ColleagueFinanceReferenceDataRepository actualRepository;
        private TestColleagueFinanceReferenceDataRepository expectedRepository;
        private CommodityCodes expectedCommodityCode;

        [TestInitialize]
        public void Initialize()
        {
            this.MockInitialize();

            // Initialize the journal entry repository
            expectedRepository = new TestColleagueFinanceReferenceDataRepository();

            this.actualRepository = BuildRepository();

            // Mock BulkReadRecord to return pre-defined AP Tax data contracts
            dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.ApTaxes>(It.IsAny<string>(), "", true)).Returns(() =>
            {
                return Task.FromResult(this.expectedRepository.ApTaxesDataContracts);
            });

            // Mock BulkReadRecord to return pre-defined AP Type data contracts
            dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<ApTypes>(It.IsAny<string>(), "", true)).Returns(() =>
            {
                return Task.FromResult(this.expectedRepository.ApTypesDataContracts);
            });

            // Get the taxform entities from the mock repository           
            var taxForms = new TestColleagueFinanceReferenceDataRepository().GetTaxFormsAsync().Result;
            // build the valcode response from the taxform entities
            var taxFormsValcodeResponse = BuildValcodeResponse(taxForms);
            // mock the response from datareader
            dataReaderMock.Setup(r => r.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "TAX.FORMS", It.IsAny<bool>())).ReturnsAsync(taxFormsValcodeResponse);
            // Get the fixedAssetsFlag entities from the mock repository           
            var fixedAssetsFlags = new TestColleagueFinanceReferenceDataRepository().GetFixedAssetTransferFlagsAsync().Result;
            // build the valcode response from the fixedAssetsFlag entities
            var fixedAssetsFlagsValcodeResponse = BuildValcodeResponse(fixedAssetsFlags);
            // mock the response from datareader
            dataReaderMock.Setup(r => r.ReadRecordAsync<ApplValcodes>("CF.VALCODES", "FXA.TRANSFER.FLAGS", It.IsAny<bool>())).ReturnsAsync(fixedAssetsFlagsValcodeResponse);
            expectedCommodityCode = new TestColleagueFinanceReferenceDataRepository().GetCommodityCodesDataByCodeAsync("10900").Result;
            // return a valid response to the data accessor request
            dataReaderMock.Setup(acc => acc.ReadRecordAsync<CommodityCodes>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(expectedCommodityCode);
        }

        [TestCleanup]
        public void Cleanup()
        {
            expectedRepository = null;
        }

        #endregion

        #region Tests

        [TestMethod]
        public async Task GetApTaxes()
        {
            var expectedCodes = await this.expectedRepository.GetAccountsPayableTaxCodesAsync();
            var actualCodes = await this.actualRepository.GetAccountsPayableTaxCodesAsync();

            foreach (var expectedCode in expectedCodes)
            {
                Assert.IsTrue(actualCodes.Any(actualCode =>
                    actualCode.Code == expectedCode.Code
                    && actualCode.Description == expectedCode.Description
                    && actualCode.AllowAccountsPayablePurchaseEntry == expectedCode.AllowAccountsPayablePurchaseEntry
                    && actualCode.IsUseTaxCategory == expectedCode.IsUseTaxCategory
                    && actualCode.TaxCategory == expectedCode.TaxCategory));
            }
        }

        [TestMethod]
        public async Task GetApTypes()
        {
            var expectedCodes = await this.expectedRepository.GetAccountsPayableTypeCodesAsync();
            var actualCodes = await this.actualRepository.GetAccountsPayableTypeCodesAsync();

            foreach (var expectedCode in expectedCodes)
            {
                Assert.IsTrue(actualCodes.Any(actualCode =>
                    actualCode.Code == expectedCode.Code
                    && actualCode.Description == expectedCode.Description));
            }
        }

        [TestMethod]
        public async Task GetTaxFormsAsync()
        {
            var expectedCodes = await this.expectedRepository.GetTaxFormsAsync();
            var actualCodes = await this.actualRepository.GetTaxFormsAsync();

            foreach (var expectedCode in expectedCodes)
            {
                Assert.IsTrue(actualCodes.Any(actualCode =>
                    actualCode.Code == expectedCode.Code
                    && actualCode.Description == expectedCode.Description));
            }
        }

        [TestMethod]
        public async Task GetFixedAssetTransferFlagsAsync()
        {
            var expectedCodes = await this.expectedRepository.GetFixedAssetTransferFlagsAsync();
            var actualCodes = await this.actualRepository.GetFixedAssetTransferFlagsAsync();

            foreach (var expectedCode in expectedCodes)
            {
                Assert.IsTrue(actualCodes.Any(actualCode =>
                    actualCode.Code == expectedCode.Code
                    && actualCode.Description == expectedCode.Description));
            }
        }

        [TestMethod]
        public async Task GetCommodityCodeByCodeAsync_Success()
        {
            string commodityCode = "10900";
            var actualCode = await this.actualRepository.GetCommodityCodeByCodeAsync(commodityCode);
            Assert.IsTrue(actualCode.Code == expectedCommodityCode.Recordkey);
            Assert.IsTrue(actualCode.Description == expectedCommodityCode.CmdtyDesc);
            Assert.IsTrue(actualCode.DefaultDescFlag == true);
            Assert.IsTrue(actualCode.FixedAssetsFlag == expectedCommodityCode.CmdtyFixedAssetsFlag);
            Assert.IsTrue(actualCode.Price == expectedCommodityCode.CmdtyPrice);
            Assert.IsTrue(actualCode.TaxCodes.Count == expectedCommodityCode.CmdtyTaxCodes.Count);
        }

        [TestMethod]
        public async Task GetCommodityCodeByCodeAsync_CommodityCode_IsNull()
        {
            string commodityCode = "C1";

            CommodityCodes nullCommodityCode = null;
            // return a valid response to the data accessor request
            dataReaderMock.Setup(acc => acc.ReadRecordAsync<CommodityCodes>("C1", It.IsAny<bool>())).ReturnsAsync(nullCommodityCode);
            var actualCode = await this.actualRepository.GetCommodityCodeByCodeAsync(commodityCode);
            Assert.IsNull(actualCode);
        }


        private ApplValcodes BuildValcodeResponse(IEnumerable<dynamic> codeItems)
        {
            ApplValcodes valCodeResponse = new ApplValcodes();
            valCodeResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
            foreach (var item in codeItems)
            {
                valCodeResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "", item.Code, "", "", ""));
            }
            return valCodeResponse;
        }
        #endregion

        #region Private methods

        private ColleagueFinanceReferenceDataRepository BuildRepository()
        {
            return new ColleagueFinanceReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
        }

        #endregion

        #region AssetCategoriesTests
        /// <summary>
        /// Test class for AssetCategories codes
        /// </summary>
        [TestClass]
        public class AssetCategoriesTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<Domain.ColleagueFinance.Entities.AssetCategories> _fixedAssetCategoriesCollection;
            string codeItemName;

            ColleagueFinanceReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build responses used for mocking
                _fixedAssetCategoriesCollection = new List<Domain.ColleagueFinance.Entities.AssetCategories>()
                {
                    new Domain.ColleagueFinance.Entities.AssetCategories("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Domain.ColleagueFinance.Entities.AssetCategories("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.ColleagueFinance.Entities.AssetCategories("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

                // Build repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                codeItemName = referenceDataRepo.BuildFullCacheKey("AllAssetCategories");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                _fixedAssetCategoriesCollection = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task GetsAssetCategoriesCacheAsync()
            {
                var result = await referenceDataRepo.GetAssetCategoriesAsync(false);

                for (int i = 0; i < _fixedAssetCategoriesCollection.Count(); i++)
                {
                    Assert.AreEqual(_fixedAssetCategoriesCollection.ElementAt(i).Guid, result.ElementAt(i).Guid);
                    Assert.AreEqual(_fixedAssetCategoriesCollection.ElementAt(i).Code, result.ElementAt(i).Code);
                    Assert.AreEqual(_fixedAssetCategoriesCollection.ElementAt(i).Description, result.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetsAssetCategoriesNonCacheAsync()
            {
                var result = await referenceDataRepo.GetAssetCategoriesAsync(true);

                for (int i = 0; i < _fixedAssetCategoriesCollection.Count(); i++)
                {
                    Assert.AreEqual(_fixedAssetCategoriesCollection.ElementAt(i).Guid, result.ElementAt(i).Guid);
                    Assert.AreEqual(_fixedAssetCategoriesCollection.ElementAt(i).Code, result.ElementAt(i).Code);
                    Assert.AreEqual(_fixedAssetCategoriesCollection.ElementAt(i).Description, result.ElementAt(i).Description);
                }
            }

            private ColleagueFinanceReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to AssetCategories read
                var entityCollection = new Collection<DataContracts.AssetCategories>(_fixedAssetCategoriesCollection.Select(record =>
                    new DataContracts.AssetCategories()
                    {
                        Recordkey = record.Code,
                        AsctDesc = record.Description                        
                    }).ToList());

                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.AssetCategories>("ASSET.CATEGORIES", "", true))
                    .ReturnsAsync(entityCollection);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));


                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var entity = _fixedAssetCategoriesCollection.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "ASSET.CATEGORIES", entity.Code }),
                            new RecordKeyLookupResult() { Guid = entity.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new ColleagueFinanceReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

                return referenceDataRepo;
            }
        }

        #endregion

        #region AssetTypesTests

        /// <summary>
        /// Test class for AssetTypes codes
        /// </summary>
        [TestClass]
        public class AssetTypesTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<Domain.ColleagueFinance.Entities.AssetTypes> _fixedAssetTypesCollection;
            Collection<DataContracts.AssetTypes> datacontractCollection;
            string codeItemName;

            ColleagueFinanceReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build responses used for mocking
                _fixedAssetTypesCollection = new List<Domain.ColleagueFinance.Entities.AssetTypes>()
                {
                    new Domain.ColleagueFinance.Entities.AssetTypes("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic") { AstpCalcMethod = "AB" },
                    new Domain.ColleagueFinance.Entities.AssetTypes("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.ColleagueFinance.Entities.AssetTypes("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

                // Build repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                codeItemName = referenceDataRepo.BuildFullCacheKey("AllAssetTypes");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                _fixedAssetTypesCollection = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task GetsAssetTypesCacheAsync()
            {
                var result = await referenceDataRepo.GetAssetTypesAsync(false);

                for (int i = 0; i < _fixedAssetTypesCollection.Count(); i++)
                {
                    Assert.AreEqual(_fixedAssetTypesCollection.ElementAt(i).Guid, result.ElementAt(i).Guid);
                    Assert.AreEqual(_fixedAssetTypesCollection.ElementAt(i).Code, result.ElementAt(i).Code);
                    Assert.AreEqual(_fixedAssetTypesCollection.ElementAt(i).Description, result.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetsAssetTypesNonCacheAsync()
            {
                var result = await referenceDataRepo.GetAssetTypesAsync(true);

                for (int i = 0; i < _fixedAssetTypesCollection.Count(); i++)
                {
                    Assert.AreEqual(_fixedAssetTypesCollection.ElementAt(i).Guid, result.ElementAt(i).Guid);
                    Assert.AreEqual(_fixedAssetTypesCollection.ElementAt(i).Code, result.ElementAt(i).Code);
                    Assert.AreEqual(_fixedAssetTypesCollection.ElementAt(i).Description, result.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetsAssetTypesNonCacheAsync_Null_DataContract()
            {
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.AssetTypes>("ASSET.TYPES", "", true))
                    .ReturnsAsync(null);
                var result = await referenceDataRepo.GetAssetTypesAsync(true);
                Assert.IsNotNull(result);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetsAssetTypesNonCacheAsync_KeyNotFoundException()
            {
                var secondElementWithWrongCalcMethodName = datacontractCollection.ElementAt(1).AstpCalcMethod = "BadCode";
                var result = await referenceDataRepo.GetAssetTypesAsync(true);
            }

            private ColleagueFinanceReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to AssetTypes read
                datacontractCollection = new Collection<DataContracts.AssetTypes>(_fixedAssetTypesCollection.Select(record =>
                    new Data.ColleagueFinance.DataContracts.AssetTypes()
                    {
                        Recordkey = record.Code,
                        AstpDesc = record.Description,
                        RecordGuid = record.Guid,
                        AstpCalcMethod = record.AstpCalcMethod
                    }).ToList());

                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.CalcMethods>("CALC.METHODS", string.Empty, true)).ReturnsAsync(new Collection<CalcMethods>()
                {
                    new CalcMethods()
                    {
                        Recordkey = "AB",
                        CalcDesc = "Account Balance"
                    }
                });

                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.AssetTypes>("ASSET.TYPES", "", true))
                    .ReturnsAsync(datacontractCollection);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));


                //dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                //{
                //    var result = new Dictionary<string, RecordKeyLookupResult>();
                //    foreach (var recordKeyLookup in recordKeyLookups)
                //    {
                //        var entity = _fixedAssetTypesCollection.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                //        result.Add(string.Join("+", new string[] { "ASSET.TYPES", entity.Code }),
                //            new RecordKeyLookupResult() { Guid = entity.Guid });
                //    }
                //    return Task.FromResult(result);
                //});

                // Construct repository
                referenceDataRepo = new ColleagueFinanceReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

                return referenceDataRepo;
            }
        }

        #endregion

        [TestClass]
        public class AccountComponentsTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<AccountComponents> allAccountComponents;
            ApplValcodes accountComponentsValcodeResponse;
            string valcodeName;
            

            Mock<IColleagueFinanceReferenceDataRepository> referenceDataRepositoryMock;
            IColleagueFinanceReferenceDataRepository referenceDataRepository;
            ColleagueFinanceReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
               
           
                allAccountComponents = new TestColleagueFinanceReferenceDataRepository().GetAccountComponentsAsync(false).Result;
                accountComponentsValcodeResponse = BuildValcodeResponse(allAccountComponents);
                var accountComponentsValResponse = new List<string>() { "2" };
                accountComponentsValcodeResponse.ValActionCode1 = accountComponentsValResponse;

                referenceDataRepositoryMock = new Mock<IColleagueFinanceReferenceDataRepository>();
                referenceDataRepository = referenceDataRepositoryMock.Object;

                // Build academic standing repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("CF_ACCOUNT.COMPONENTS_GUID");

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                   x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                   .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                accountComponentsValcodeResponse = null;
                allAccountComponents = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task ColleagueFinanceReferenceData_GetsAccountComponentsNoArgAsync()
            {
                var accountComponents = await referenceDataRepo.GetAccountComponentsAsync(true);

                for (int i = 0; i < allAccountComponents.Count(); i++)
                {
                    Assert.AreEqual(allAccountComponents.ElementAt(i).Code, accountComponents.ElementAt(i).Code);
                    Assert.AreEqual(allAccountComponents.ElementAt(i).Description, accountComponents.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task ColleagueFinanceReferenceData_GetsAccountComponentsCacheAsync()
            {
                var accountComponents = await referenceDataRepo.GetAccountComponentsAsync(false);

                for (int i = 0; i < allAccountComponents.Count(); i++)
                {
                    Assert.AreEqual(allAccountComponents.ElementAt(i).Code, accountComponents.ElementAt(i).Code);
                    Assert.AreEqual(allAccountComponents.ElementAt(i).Description, accountComponents.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task ColleagueFinanceReferenceData_GetsAccountComponentsNonCacheAsync()
            {
                var statuses = await referenceDataRepo.GetAccountComponentsAsync(true);

                for (int i = 0; i < allAccountComponents.Count(); i++)
                {
                    Assert.AreEqual(allAccountComponents.ElementAt(i).Code, statuses.ElementAt(i).Code);
                    Assert.AreEqual(allAccountComponents.ElementAt(i).Description, statuses.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task ColleagueFinanceReferenceData_GetAccountComponents_WritesToCacheAsync()
            {

                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CF.VALCODES", "ACCOUNT.COMPONENTS", It.IsAny<bool>())).ReturnsAsync(accountComponentsValcodeResponse);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // But after data accessor read, set up mocking so we can verify the list of accountComponents was written to the cache
                cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<List<AccountComponents>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                cacheProviderMock.Setup(x => x.Contains(referenceDataRepo.BuildFullCacheKey("CF_ACCOUNT.COMPONENTS"), null)).Returns(true);
                var accountComponents = await referenceDataRepo.GetAccountComponentsAsync(false);
                cacheProviderMock.Setup(x => x.Get(referenceDataRepo.BuildFullCacheKey("CF_ACCOUNT.COMPONENTS"), null)).Returns(accountComponents);
                // Verify that accountComponents were returned, which means they came from the "repository".
                Assert.IsTrue(accountComponents.Count() == 2);

                // Verify that the accountComponents item was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<List<AccountComponents>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);

            }

            [TestMethod]
            public async Task ColleagueFinanceReferenceData_GetAccountComponents_GetsCachedAccountComponentsAsync()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item (in this case the "ACCOUNT.COMPONENTS" cache item)
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(true);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(allAccountComponents).Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CF.VALCODES", "ACCOUNT.COMPONENTS", true)).ReturnsAsync(new ApplValcodes());

                // Assert the accountComponents are returned
                Assert.IsTrue((await referenceDataRepo.GetAccountComponentsAsync(false)).Count() == 2);
                // Verify that the saccountComponents were retrieved from cache
                cacheProviderMock.Verify(m => m.Get(valcodeName, null));
            }

            private ColleagueFinanceReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to accountComponents valcode read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CF.VALCODES", "ACCOUNT.COMPONENTS", It.IsAny<bool>())).ReturnsAsync(accountComponentsValcodeResponse);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var accountComponents = allAccountComponents.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "CF.VALCODES", "ACCOUNT.COMPONENTS", accountComponents.Code }),
                            new RecordKeyLookupResult() { Guid = accountComponents.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new ColleagueFinanceReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<AccountComponents> accountComponents)
            {
                ApplValcodes accountComponentsResponse = new ApplValcodes();
                accountComponentsResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in accountComponents)
                {
                    accountComponentsResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "2", item.Code, "3", "", ""));
                }
                return accountComponentsResponse;
            }
        }

        /// <summary>
        /// Test class for VendorHoldReasons
        /// </summary>
        [TestClass]
        public class GlSourceCodesTests
        {
            private Mock<IColleagueTransactionFactory> transFactoryMock;
            private Mock<ICacheProvider> cacheProviderMock;
            private Mock<IColleagueDataReader> dataAccessorMock;
            private Mock<ILogger> loggerMock;
            private IEnumerable<GlSourceCodes> allGlSourceCodesEntities;
            private ApplValcodes intgGlSourceCodes;
            private string valcodeName;

            private ColleagueFinanceReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build responses used for mocking
                BuildData();
                intgGlSourceCodes = BuildValcodeResponse(allGlSourceCodesEntities);

                // Build repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("CF_GL_SOURCE_CODES_GUID");

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                    x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));
            }

            private void BuildData()
            {
                allGlSourceCodesEntities = new List<GlSourceCodes>() 
                {
                    new GlSourceCodes("6e274e84-2cba-4f11-8404-be7a23e65663", "Encumbranceopenbalance", "Desc 1", "Encumbranceopenbalance"),
                    new GlSourceCodes("2137e2e2-21d5-49e3-a676-c429da9bbc38", "Studentinvoice", "Desc 2", "Studentinvoice")
                };
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                allGlSourceCodesEntities = null;
                referenceDataRepo = null;
                intgGlSourceCodes = null;
            }

            [TestMethod]
            public async Task GetsGlSourceCodesCacheAsync()
            {
                var glSourceCodes = await referenceDataRepo.GetGlSourceCodesValcodeAsync(false);

                for (int i = 0; i < allGlSourceCodesEntities.Count(); i++)
                {
                    Assert.AreEqual(allGlSourceCodesEntities.ElementAt(i).Guid, glSourceCodes.ElementAt(i).Guid);
                    Assert.AreEqual(allGlSourceCodesEntities.ElementAt(i).Code, glSourceCodes.ElementAt(i).Code);
                    Assert.AreEqual(allGlSourceCodesEntities.ElementAt(i).Description, glSourceCodes.ElementAt(i).Description);

                }
            }

            [TestMethod]
            public async Task GetsGlSourceCodesNonCacheAsync()
            {
                var glSourceCodes = await referenceDataRepo.GetGlSourceCodesValcodeAsync(true);

                for (int i = 0; i < allGlSourceCodesEntities.Count(); i++)
                {
                    Assert.AreEqual(allGlSourceCodesEntities.ElementAt(i).Guid, glSourceCodes.ElementAt(i).Guid);
                    Assert.AreEqual(allGlSourceCodesEntities.ElementAt(i).Code, glSourceCodes.ElementAt(i).Code);
                    Assert.AreEqual(allGlSourceCodesEntities.ElementAt(i).Description, glSourceCodes.ElementAt(i).Description);

                }
            }
            private ApplValcodes BuildValcodeResponse(IEnumerable<GlSourceCodes> glSourceCodes)
            {
                ApplValcodes glSourceCodesResponse = new ApplValcodes();
                glSourceCodesResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in glSourceCodes)
                {
                    glSourceCodesResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "2", item.Code, "3", item.GlSourceCodeProcess3, ""));
                }
                return glSourceCodesResponse;
            }

            private ColleagueFinanceReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to vendorHoldReasons valcode read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CF.VALCODES", "GL.SOURCE.CODES", It.IsAny<bool>())).ReturnsAsync(intgGlSourceCodes);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var glSourceCode = allGlSourceCodesEntities.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "CF.VALCODES", "GL.SOURCE.CODES", glSourceCode.Code }),
                            new RecordKeyLookupResult() { Guid = glSourceCode.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new ColleagueFinanceReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

                return referenceDataRepo;
            }
        }

        /// <summary>
        /// AccountingStringComponentValuesTests
        /// </summary>
        [TestClass]
        public class AccountingStringComponentValuesTests : BaseRepositorySetup
        {
            private ColleagueFinanceReferenceDataRepository _referenceDataRepo;
            private Mock<ColleagueFinanceReferenceDataRepository> _referenceDataRepoMock;
            private Collection<DataContracts.GlAccts> _glAcctsDataContract;
            private DataContracts.Glclsdef _glClsdefDataContract;
            private Collection<DataContracts.GlAcctsCc> _glAcctsCcDataContract;
            private Fiscalyr _fiscalYrDataContract;
            private Collection<DataContracts.Projects> _projectsDataContract;

         
            private string criteria = "";
            private string[] ids = new string[] {"1", "2", "3", "4"};
            private string expectedRecordKey = "1";
            private string guid = "4f937f08-f6a0-4a1c-8d55-9f2a6dd6be46";
            private int offset = 0;
            private int limit = 4;

            [TestInitialize]
            public async void Initialize()
            {
                MockInitialize();
                _referenceDataRepo = new ColleagueFinanceReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
                _referenceDataRepoMock = new Mock<ColleagueFinanceReferenceDataRepository>();
                BuildData();

                var glIds = _glAcctsDataContract.Select(i => i.Recordkey).ToArray();
                var prIds = _projectsDataContract.Select(i => i.Recordkey).ToArray();

                dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.GlAccts>("GL.ACCTS", It.IsAny<string[]>(), It.IsAny<bool>()))
                    .ReturnsAsync(_glAcctsDataContract);
                dataReaderMock.Setup(acc => acc.SelectAsync("GL.ACCTS", string.Empty)).ReturnsAsync(glIds);

                dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.GlAccts>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                    .ReturnsAsync(_glAcctsDataContract);

                dataReaderMock.Setup(acc => acc.ReadRecordAsync<DataContracts.Glclsdef>("ACCOUNT.PARAMETERS", "GL.CLASS.DEF", It.IsAny<bool>()))
                    .ReturnsAsync(_glClsdefDataContract);

                dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.GlAcctsCc>("GL.ACCTS.CC", "", It.IsAny<bool>()))
                    .ReturnsAsync(_glAcctsCcDataContract);

                dataReaderMock.Setup(acc => acc.ReadRecordAsync<DataContracts.Fiscalyr>("ACCOUNT.PARAMETERS", "FISCAL.YEAR", It.IsAny<bool>()))
                    .ReturnsAsync(_fiscalYrDataContract);


                dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.Projects>("PROJECTS", It.IsAny<string[]>(), It.IsAny<bool>()))
                    .ReturnsAsync(_projectsDataContract);
                dataReaderMock.Setup(acc => acc.SelectAsync("PROJECTS", string.Empty)).ReturnsAsync(prIds);


                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                    x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .Returns(Task.FromResult(new Tuple<object, SemaphoreSlim>(
                        null,
                        new SemaphoreSlim(1, 1)
                        )));
            }

            [TestCleanup]
            public void Cleanup()
            {
                MockCleanup();
                _referenceDataRepo = null;
                _referenceDataRepoMock = null;
            }


            [TestMethod]
            public async Task ColleagueFinanceReferenceData_GetAccountingStringComponentValueByGuid_Projects()
            {
                var projectGuid = "A42A664D-70FC-4EE9-B31C-B57B652E719B";
                dataReaderMock.Setup(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
                {
                    var result = new Dictionary<string, GuidLookupResult>();
                    foreach (var gl in gla)
                    {
                        result.Add(gl.Guid, new GuidLookupResult() { Entity = "PROJECTS", PrimaryKey = "1" });
                    }
                    return Task.FromResult(result);
                });

                dataReaderMock.Setup(acc => acc.ReadRecordAsync<DataContracts.Projects>(It.IsAny<string>(), It.IsAny<bool>()))
                  .ReturnsAsync(_projectsDataContract.FirstOrDefault(p => p.Recordkey == "1"));

                var actuals = await _referenceDataRepo.GetAccountingStringComponentValueByGuid(projectGuid);
                Assert.IsNotNull(actuals);
            }

            [TestMethod]
            public async Task ColleagueFinanceReferenceData_GetAccountingStringComponentValueByGuid_GlAccts()
            {
                dataReaderMock.Setup(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
                {
                    var result = new Dictionary<string, GuidLookupResult>();
                    foreach (var gl in gla)
                    {
                        result.Add(gl.Guid, new GuidLookupResult() { Entity = "GL.ACCTS", PrimaryKey = "11_00_01_00_20603_52010" });
                    }
                    return Task.FromResult(result);
                });

                dataReaderMock.Setup(acc => acc.ReadRecordAsync<DataContracts.GlAccts>(It.IsAny<string>(), It.IsAny<bool>()))
                    .ReturnsAsync(_glAcctsDataContract.FirstOrDefault(p => p.Recordkey == "11_00_01_00_20603_52010"));

                var actuals = await _referenceDataRepo.GetAccountingStringComponentValueByGuid(guid);
                Assert.IsNotNull(actuals);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task ColleagueFinanceReferenceData_GetAccountingStringComponentValueByGuid_KeyNotFound()
            {
                dataReaderMock.Setup(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
                {
                    var result = new Dictionary<string, GuidLookupResult>();
                    foreach (var gl in gla)
                    {
                        result.Add(gl.Guid, new GuidLookupResult() { Entity = "", PrimaryKey = "" });
                    }
                    return Task.FromResult(result);
                });
               await _referenceDataRepo.GetAccountingStringComponentValueByGuid(guid);
                
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task ColleagueFinanceReferenceData_GetAccountingStringComponentValueByGuid_InvalidEntity()
            {
                dataReaderMock.Setup(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
                {
                    var result = new Dictionary<string, GuidLookupResult>();
                    foreach (var gl in gla)
                    {
                        result.Add(gl.Guid, new GuidLookupResult() { Entity = "INVALID", PrimaryKey = "1" });
                    }
                    return Task.FromResult(result);
                });
                await _referenceDataRepo.GetAccountingStringComponentValueByGuid(guid);

            }

            [TestMethod]
            public async Task ColleagueFinanceReferenceData_GetAccountingStringComponentValueByGuid_AccountFundsController()
            {
                var record = _glAcctsDataContract.FirstOrDefault(p => p.Recordkey == "11_00_01_00_20603_52010");
                record.MemosEntityAssociation = new List<GlAcctsMemos>() { new GlAcctsMemos()};
                dataReaderMock.Setup(acc => acc.ReadRecordAsync<DataContracts.GlAccts>(It.IsAny<string>(), It.IsAny<bool>()))
                   .ReturnsAsync(_glAcctsDataContract.FirstOrDefault(p => p.Recordkey == "11_00_01_00_20603_52010"));
                dataReaderMock.Setup(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
                {
                    var result = new Dictionary<string, GuidLookupResult>();
                    foreach (var gl in gla)
                    {
                        result.Add(gl.Guid, new GuidLookupResult() { Entity = "GL.ACCTS", PrimaryKey = "1" });
                    }
                    return Task.FromResult(result);
                });
                try
                {
                    await _referenceDataRepo.GetAccountingStringComponentValueByGuid(record.RecordGuid);
                }
                catch (RepositoryException e)
                {
                    Assert.AreEqual(1, e.Errors.Count());
                    Assert.AreEqual("The record associated to the accounting string component value contains an invalid element. guid: '4f937f08-f6a0-4a1c-8d55-9f2a6dd6be46'",
                        e.Errors[0].Message);
                }

            }

            [TestMethod]
            public async Task ColleagueFinanceReferenceData_GetAccountingStringComponentValues_Component_All_NonCache()
            {
                var actuals = await _referenceDataRepo.GetAccountingStringComponentValues2Async(offset, limit, "", "", "");
                Assert.IsNotNull(actuals);
                var actual = actuals.Item1.FirstOrDefault(a => a.Guid == guid);
                var expected = _glAcctsDataContract.FirstOrDefault(glAcct => glAcct.RecordGuid == guid);
                var expectedCc = _glAcctsCcDataContract.FirstOrDefault(cc => cc.Recordkey == expected.Recordkey);
                Assert.IsNotNull(actual);
                Assert.IsNotNull(expected);
                Assert.IsNotNull(expectedCc);
                Assert.AreEqual(guid, actual.Guid);
                Assert.AreEqual(expectedCc.GlccAcctDesc, actual.Description);
                Assert.AreEqual("available", actual.Status);
                Assert.AreEqual("expense", actual.Type);
                Assert.AreEqual("GL", actual.AccountDef);
                Assert.AreEqual(expected.Recordkey, actual.AccountNumber);
            }

            [TestMethod]
            public async Task ColleagueFinanceReferenceData_GetAccountingStringComponentValues_Component_GlAcct_NonCache()
            {
                var actuals = await _referenceDataRepo.GetAccountingStringComponentValues2Async(offset, limit, "GL.ACCT", "", "");
                Assert.IsNotNull(actuals);
                var actual = actuals.Item1.FirstOrDefault(a => a.Guid == guid);
                var expected = _glAcctsDataContract.FirstOrDefault(glAcct => glAcct.RecordGuid == guid);
                var expectedCc = _glAcctsCcDataContract.FirstOrDefault(cc => cc.Recordkey == expected.Recordkey);
                Assert.IsNotNull(actual);
                Assert.IsNotNull(expected);
                Assert.IsNotNull(expectedCc);
                Assert.AreEqual(guid, actual.Guid);
                Assert.AreEqual(expectedCc.GlccAcctDesc, actual.Description);
                Assert.AreEqual("available", actual.Status);
                Assert.AreEqual("expense", actual.Type);
                Assert.AreEqual("GL", actual.AccountDef);
                Assert.AreEqual(expected.Recordkey, actual.AccountNumber);
            }

            [TestMethod]
            public async Task ColleagueFinanceReferenceData_GetAccountingStringComponentValues_Component_Project_NonCache()
            {
                var actuals = await _referenceDataRepo.GetAccountingStringComponentValues2Async(offset, limit, "PROJECT", "", "");
                Assert.IsNotNull(actuals);
            }

            [TestMethod]
            public async Task ColleagueFinanceReferenceData_GetAccountingStringComponentValues_Component_NullProjectIds()
            {
                dataReaderMock.Setup(acc => acc.SelectAsync("PROJECTS", string.Empty)).ReturnsAsync(null);
                var actuals = await _referenceDataRepo.GetAccountingStringComponentValues2Async(offset, limit, "PROJECT", "", "");
                Assert.IsNotNull(actuals);
                Assert.AreEqual(0, actuals.Item2);
            }

            [TestMethod]
            public async Task ColleagueFinanceReferenceData_GetAccountingStringComponentValues_Component_NullGLAcctsIds()
            {
                dataReaderMock.Setup(acc => acc.SelectAsync("GL.ACCTS", string.Empty)).ReturnsAsync(null);
                var actuals = await _referenceDataRepo.GetAccountingStringComponentValues2Async(offset, limit, "GL.ACCT", "", "");
                Assert.IsNotNull(actuals);
                Assert.AreEqual(0, actuals.Item2);
            }

            [TestMethod]
            public async Task ColleagueFinanceReferenceData_GetAccountingStringComponentValues_Component_NullGLAcctsPrjIds()
            {
                dataReaderMock.Setup(acc => acc.SelectAsync("GL.ACCTS", string.Empty)).ReturnsAsync(null);
                dataReaderMock.Setup(acc => acc.SelectAsync("PROJECTS", string.Empty)).ReturnsAsync(null);

                var actuals = await _referenceDataRepo.GetAccountingStringComponentValues2Async(offset, limit, "", "", "");
                Assert.IsNotNull(actuals);
                Assert.AreEqual(0, actuals.Item2);
            }

            [TestMethod]
            public async Task ColleagueFinanceReferenceData_GetAccountingStringComponentValues_Component_WithTransactionStatus()
            {
                var glIds = _glAcctsDataContract.Select(i => i.Recordkey).ToArray();
                var prIds = _projectsDataContract.Select(i => i.Recordkey).ToArray();
                dataReaderMock.Setup(acc => acc.SelectAsync("GL.ACCTS", string.Empty)).ReturnsAsync(glIds);
                dataReaderMock.Setup(acc => acc.SelectAsync("PROJECTS", string.Empty)).ReturnsAsync(prIds);

                var actuals = await _referenceDataRepo.GetAccountingStringComponentValues2Async(offset, limit, "", "available", "");
                Assert.IsNotNull(actuals);
                Assert.AreEqual(0, actuals.Item2);
            }

            [TestMethod]
            public async Task ColleagueFinanceReferenceData_GetAccountingStringComponentValues_Component_AssetTypeAccount()
            {
                var glIds = _glAcctsDataContract.Select(i => i.Recordkey).ToArray();
                var prIds = _projectsDataContract.Select(i => i.Recordkey).ToArray();
                dataReaderMock.Setup(acc => acc.SelectAsync("GL.ACCTS", string.Empty)).ReturnsAsync(glIds);
                dataReaderMock.Setup(acc => acc.SelectAsync("PROJECTS", string.Empty)).ReturnsAsync(prIds);

                var actuals = await _referenceDataRepo.GetAccountingStringComponentValues2Async(offset, limit, "", "", "asset");
                Assert.IsNotNull(actuals);
                Assert.AreEqual(1, actuals.Item2);
            }

            [TestMethod]
            public async Task ColleagueFinanceReferenceData_GetAccountingStringComponentValues_Component_LiabilityTypeAccount()
            {
                var glIds = _glAcctsDataContract.Select(i => i.Recordkey).ToArray();
                var prIds = _projectsDataContract.Select(i => i.Recordkey).ToArray();
                dataReaderMock.Setup(acc => acc.SelectAsync("GL.ACCTS", string.Empty)).ReturnsAsync(glIds);
                dataReaderMock.Setup(acc => acc.SelectAsync("PROJECTS", string.Empty)).ReturnsAsync(prIds);

                var actuals = await _referenceDataRepo.GetAccountingStringComponentValues2Async(offset, limit, "", "", "liability");
                Assert.IsNotNull(actuals);
                Assert.AreEqual(0, actuals.Item2);
            }

            [TestMethod]
            public async Task ColleagueFinanceReferenceData_GetAccountingStringComponentValues_Component_revenueTypeAccount()
            {
                var glIds = _glAcctsDataContract.Select(i => i.Recordkey).ToArray();
                var prIds = _projectsDataContract.Select(i => i.Recordkey).ToArray();
                dataReaderMock.Setup(acc => acc.SelectAsync("GL.ACCTS", string.Empty)).ReturnsAsync(glIds);
                dataReaderMock.Setup(acc => acc.SelectAsync("PROJECTS", string.Empty)).ReturnsAsync(prIds);

                var actuals = await _referenceDataRepo.GetAccountingStringComponentValues2Async(offset, limit, "", "", "revenue");
                Assert.IsNotNull(actuals);
                Assert.AreEqual(0, actuals.Item2);
            }

            [TestMethod]
            public async Task ColleagueFinanceReferenceData_GetAccountingStringComponentValues_Component_FundBalanceTypeAccount()
            {
                var glIds = _glAcctsDataContract.Select(i => i.Recordkey).ToArray();
                var prIds = _projectsDataContract.Select(i => i.Recordkey).ToArray();
                dataReaderMock.Setup(acc => acc.SelectAsync("GL.ACCTS", string.Empty)).ReturnsAsync(glIds);
                dataReaderMock.Setup(acc => acc.SelectAsync("PROJECTS", string.Empty)).ReturnsAsync(prIds);

                var actuals = await _referenceDataRepo.GetAccountingStringComponentValues2Async(offset, limit, "", "", "fundBalance");
                Assert.IsNotNull(actuals);
                Assert.AreEqual(0, actuals.Item2);
            }

            [TestMethod]
            public async Task ColleagueFinanceReferenceData_GetAccountingStringComponentValues_Component_ExpenseTypeAccount()
            {
                var glIds = _glAcctsDataContract.Select(i => i.Recordkey).ToArray();
                var prIds = _projectsDataContract.Select(i => i.Recordkey).ToArray();
                dataReaderMock.Setup(acc => acc.SelectAsync("GL.ACCTS", string.Empty)).ReturnsAsync(glIds);
                dataReaderMock.Setup(acc => acc.SelectAsync("PROJECTS", string.Empty)).ReturnsAsync(prIds);

                var actuals = await _referenceDataRepo.GetAccountingStringComponentValues2Async(offset, limit, "", "", "expense");
                Assert.IsNotNull(actuals);
                Assert.AreEqual(4, actuals.Item2);
            }

            [TestMethod]
            public async Task ColleagueFinanceReferenceData_GetAccountingStringComponentValues_Component_NonPersonalExpenseTypeAccount()
            {
                var glIds = _glAcctsDataContract.Select(i => i.Recordkey).ToArray();
                var prIds = _projectsDataContract.Select(i => i.Recordkey).ToArray();
                dataReaderMock.Setup(acc => acc.SelectAsync("GL.ACCTS", string.Empty)).ReturnsAsync(glIds);
                dataReaderMock.Setup(acc => acc.SelectAsync("PROJECTS", string.Empty)).ReturnsAsync(prIds);

                var actuals = await _referenceDataRepo.GetAccountingStringComponentValues2Async(offset, limit, "", "", "nonPersonalExpense");
                Assert.IsNotNull(actuals);
                Assert.AreEqual(0, actuals.Item2);
            }

            [TestMethod]
            public async Task ColleagueFinanceReferenceData_GetAccountingStringComponentValues_Component_All_Cache()
            {
                var actuals = await _referenceDataRepo.GetAccountingStringComponentValues2Async(offset, limit, "", "", "");
                Assert.IsNotNull(actuals);
                var actual = actuals.Item1.FirstOrDefault(a => a.Guid == guid);
                var expected = _glAcctsDataContract.FirstOrDefault(glAcct => glAcct.RecordGuid == guid);
                var expectedCc = _glAcctsCcDataContract.FirstOrDefault(cc => cc.Recordkey == expected.Recordkey);
                Assert.IsNotNull(actual);
                Assert.IsNotNull(expected);
                Assert.IsNotNull(expectedCc);
                Assert.AreEqual(guid, actual.Guid);
                Assert.AreEqual(expectedCc.GlccAcctDesc, actual.Description);
                Assert.AreEqual("available", actual.Status);
                Assert.AreEqual("expense", actual.Type);
                Assert.AreEqual("GL", actual.AccountDef);
                Assert.AreEqual(expected.Recordkey, actual.AccountNumber);
            }

            [TestMethod]
            public async Task ColleagueFinanceReferenceData_GetAccountingStringComponentValues_Component_GlAcct_Cache()
            {
                var actuals = await _referenceDataRepo.GetAccountingStringComponentValues2Async(offset, limit, "GL.ACCT", "", "");
                Assert.IsNotNull(actuals);
                var actual = actuals.Item1.FirstOrDefault(a => a.Guid == guid);
                var expected = _glAcctsDataContract.FirstOrDefault(glAcct => glAcct.RecordGuid == guid);
                var expectedCc = _glAcctsCcDataContract.FirstOrDefault(cc => cc.Recordkey == expected.Recordkey);
                Assert.IsNotNull(actual);
                Assert.IsNotNull(expected);
                Assert.IsNotNull(expectedCc);
                Assert.AreEqual(guid, actual.Guid);
                Assert.AreEqual(expectedCc.GlccAcctDesc, actual.Description);
                Assert.AreEqual("available", actual.Status);
                Assert.AreEqual("expense", actual.Type);
                Assert.AreEqual("GL", actual.AccountDef);
                Assert.AreEqual(expected.Recordkey, actual.AccountNumber);
            }

            [TestMethod]
            public async Task ColleagueFinanceReferenceData_GetAccountingStringComponentValues_Component_Project_Cache()
            {
                var actuals = await _referenceDataRepo.GetAccountingStringComponentValues2Async(offset, limit, "PROJECT", "", "");
                Assert.IsNotNull(actuals);
                     }

            [TestMethod]
            public async Task ColleagueFinanceReferenceData_GetAccountingStringComponentValues_FiscalYear_Null()
            {
                _fiscalYrDataContract = null;
                var actuals = await _referenceDataRepo.GetAccountingStringComponentValues2Async(offset, limit, "PROJECT", "", "");
                Assert.IsNotNull(actuals);
               }

            [TestMethod]
            public async Task ColleagueFinanceReferenceData_GetAccountingStringComponentValues_FiscalYear_NoQualifying()
            {
                _fiscalYrDataContract = new Fiscalyr()
                {
                    Recordkey = "1",
                    CfCurrentFiscalYear = "1999",
                    FiscalStartMonth = 1
                };
                var actuals = await _referenceDataRepo.GetAccountingStringComponentValues2Async(offset, limit, "PROJECT", "", "");
                Assert.IsNotNull(actuals);
            }


            [TestMethod]
            public async Task ColleagueFinanceReferenceData_GetAccountingStringComponentValues_Project_Type()
            {
                _glClsdefDataContract = new Glclsdef()
                {
                    GlClassLocation = null
                };

                var actuals = await _referenceDataRepo.GetAccountingStringComponentValues2Async(offset, limit, "PROJECT", "", "");
                Assert.IsNotNull(actuals);
                foreach (var actual in actuals.Item1)
                {
                    Assert.AreEqual("expense", actual.Type);
                }
            }
       
             private void BuildData()
            {
                _glAcctsDataContract = new Collection<GlAccts>()
                {
                    new DataContracts.GlAccts()
                    {
                        Recordkey = "11_00_01_00_20603_52010",
                        AvailFundsController = new List<string>() {"2016", "2017"},
                        RecordGuid = guid,
                        MemosEntityAssociation = new List<DataContracts.GlAcctsMemos>()
                        {
                            new DataContracts.GlAcctsMemos()
                            {
                                AvailFundsControllerAssocMember = "2017",
                                GlFreezeFlagsAssocMember = "O",
                                GlBudgetPostedAssocMember = 100,
                                GlBudgetMemosAssocMember = 500,
                                GlActualPostedAssocMember = 10,
                                GlActualMemosAssocMember = 50,
                                GlEncumbrancePostedAssocMember = 20,
                                GlEncumbranceMemosAssocMember = 30,
                                GlRequisitionMemosAssocMember = 100

                            },
                            new DataContracts.GlAcctsMemos()
                            {
                                AvailFundsControllerAssocMember = "2016",
                                GlFreezeFlagsAssocMember = "C",
                                GlBudgetPostedAssocMember = 100,
                                GlBudgetMemosAssocMember = 500,
                                GlActualPostedAssocMember = 10,
                                GlActualMemosAssocMember = 50,
                                GlEncumbrancePostedAssocMember = 20,
                                GlEncumbranceMemosAssocMember = 30,
                                GlRequisitionMemosAssocMember = 100,

                            }
                        },
                        GlInactive = "A"

                    },
                    new DataContracts.GlAccts()
                    {
                        Recordkey = "11_00_01_00_20603_12010",
                        AvailFundsController = new List<string>() {"2017"},
                        RecordGuid = "3AA09265-F53F-4D68-85D6-BF8903362527",
                        MemosEntityAssociation = new List<DataContracts.GlAcctsMemos>()
                        {
                            new DataContracts.GlAcctsMemos()
                            {
                                AvailFundsControllerAssocMember = "2017",
                                GlFreezeFlagsAssocMember = "F",
                                GlBudgetPostedAssocMember = 100,
                                GlBudgetMemosAssocMember = 500,
                                GlActualPostedAssocMember = 10,
                                GlActualMemosAssocMember = 50,
                                GlEncumbrancePostedAssocMember = 20,
                                GlEncumbranceMemosAssocMember = 30,
                                GlRequisitionMemosAssocMember = 100

                            }
                        }
                    }
                };

                _glClsdefDataContract = new Glclsdef()
                {
                    GlClassLocation = new List<int?>() {19, 1},
                    GlClassAssetValues = new List<string>() {"1"},
                    GlClassLiabilityValues = new List<string>() {"2"},
                    GlClassFundBalValues = new List<string>() { "3" },
                    GlClassRevenueValues = new List<string>() { "4" },
                    GlClassExpenseValues = new List<string>() { "5" }
                };

                _glAcctsCcDataContract = new Collection<GlAcctsCc>()
                {
                    new GlAcctsCc()
                    {
                        GlccAcctDesc = "Description",
                        Recordkey = "11_00_01_00_20603_52010"
                    }
                };

                _fiscalYrDataContract = new Fiscalyr()
                {
                    Recordkey = "1", CfCurrentFiscalYear = "2017", FiscalStartMonth = 1
                };

                _projectsDataContract = new Collection<Projects>()
                {
                    new Projects() {RecordGuid = "A42A664D-70FC-4EE9-B31C-B57B652E719B", Recordkey = "1", PrjRefNo = "STPU-001", PrjType = "R", PrjCurrentStatus = "A", PrjTitle = "Test1"},
                    new Projects() {RecordGuid = "B42A664D-70FC-4EE9-B31C-B57B652E719B", Recordkey = "2", PrjRefNo = "STPU-002", PrjType = "A", PrjCurrentStatus = "X", PrjTitle = "Test2"},
                    new Projects() {RecordGuid = "C42A664D-70FC-4EE9-B31C-B57B652E719B", Recordkey = "3", PrjRefNo = "STPU-003", PrjType = "T", PrjCurrentStatus = "I", PrjTitle = "Test3"}

                };
            }
        }

        /// <summary>
        /// AccountingStringComponentValuesTests2
        /// </summary>
        [TestClass]
        public class AccountingStringComponentValuesTests2 : BaseRepositorySetup
        {
            private ColleagueFinanceReferenceDataRepository _referenceDataRepo;
            private Mock<ColleagueFinanceReferenceDataRepository> _referenceDataRepoMock;
            private Collection<DataContracts.GlAccts> _glAcctsDataContract;
            private DataContracts.Glclsdef _glClsdefDataContract;
            private Collection<DataContracts.GlAcctsCc> _glAcctsCcDataContract;
            private Fiscalyr _fiscalYrDataContract;
            private Collection<DataContracts.Projects> _projectsDataContract;


            private string criteria = "";
            private string[] ids = new string[] { "1", "2", "3", "4" };
            private string expectedRecordKey = "1";
            private string guid = "4f937f08-f6a0-4a1c-8d55-9f2a6dd6be46";
            private int offset = 0;
            private int limit = 4;

            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();
                _referenceDataRepo = new ColleagueFinanceReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
                _referenceDataRepoMock = new Mock<ColleagueFinanceReferenceDataRepository>();
                BuildData();

                var glIds = _glAcctsDataContract.Select(i => i.Recordkey).ToArray();
                var prIds = _projectsDataContract.Select(i => i.Recordkey).ToArray();

                dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.GlAccts>("GL.ACCTS", It.IsAny<string[]>(), It.IsAny<bool>()))
                    .ReturnsAsync(_glAcctsDataContract);
                dataReaderMock.Setup(acc => acc.SelectAsync("GL.ACCTS", string.Empty)).ReturnsAsync(glIds);
                dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.GlAccts>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                    .ReturnsAsync(_glAcctsDataContract);

                dataReaderMock.Setup(acc => acc.ReadRecordAsync<DataContracts.Glclsdef>("ACCOUNT.PARAMETERS", "GL.CLASS.DEF", It.IsAny<bool>()))
                    .ReturnsAsync(_glClsdefDataContract);

                dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.GlAcctsCc>("GL.ACCTS.CC", "", It.IsAny<bool>()))
                    .ReturnsAsync(_glAcctsCcDataContract);

                dataReaderMock.Setup(acc => acc.ReadRecordAsync<DataContracts.Fiscalyr>("ACCOUNT.PARAMETERS", "FISCAL.YEAR", It.IsAny<bool>()))
                    .ReturnsAsync(_fiscalYrDataContract);

                dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.Projects>("PROJECTS", It.IsAny<string[]>(), It.IsAny<bool>()))
                    .ReturnsAsync(_projectsDataContract);
                dataReaderMock.Setup(acc => acc.SelectAsync("PROJECTS", string.Empty)).ReturnsAsync(prIds);


                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                    x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .Returns(Task.FromResult(new Tuple<object, SemaphoreSlim>(
                        null,
                        new SemaphoreSlim(1, 1)
                        )));
            }

            [TestCleanup]
            public void Cleanup()
            {
                MockCleanup();
                _referenceDataRepo = null;
                _referenceDataRepoMock = null;
            }


            [TestMethod]
            public async Task ColleagueFinanceReferenceData_GetAccountingStringComponentValue2ByGuid_Projects()
            {
                var projectGuid = "A42A664D-70FC-4EE9-B31C-B57B652E719B";
                dataReaderMock.Setup(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
                {
                    var result = new Dictionary<string, GuidLookupResult>();
                    foreach (var gl in gla)
                    {
                        result.Add(gl.Guid, new GuidLookupResult() { Entity = "PROJECTS", PrimaryKey = "1" });
                    }
                    return Task.FromResult(result);
                });

                dataReaderMock.Setup(acc => acc.ReadRecordAsync<DataContracts.Projects>(It.IsAny<string>(), It.IsAny<bool>()))
                  .ReturnsAsync(_projectsDataContract.FirstOrDefault(p => p.Recordkey == "1"));

                var actuals = await _referenceDataRepo.GetAccountingStringComponentValue2ByGuid(projectGuid);
                Assert.IsNotNull(actuals);
            }

            [TestMethod]
            public async Task ColleagueFinanceReferenceData_GetAccountingStringComponentValue2ByGuid_GlAccts()
            {
                dataReaderMock.Setup(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
                {
                    var result = new Dictionary<string, GuidLookupResult>();
                    foreach (var gl in gla)
                    {
                        result.Add(gl.Guid, new GuidLookupResult() { Entity = "GL.ACCTS", PrimaryKey = "11_00_01_00_20603_52010" });
                    }
                    return Task.FromResult(result);
                });

                dataReaderMock.Setup(acc => acc.ReadRecordAsync<DataContracts.GlAccts>(It.IsAny<string>(), It.IsAny<bool>()))
                    .ReturnsAsync(_glAcctsDataContract.FirstOrDefault(p => p.Recordkey == "11_00_01_00_20603_52010"));

                var actuals = await _referenceDataRepo.GetAccountingStringComponentValue2ByGuid(guid);
                Assert.IsNotNull(actuals);
            }

            [TestMethod]
            public async Task ColleagueFinanceReferenceData_GetAccountingStringComponentValueByGuid2_AccountingComponent()
            {
                var record = _glAcctsDataContract.FirstOrDefault(p => p.Recordkey == "11_00_01_00_20603_52010");
                record.MemosEntityAssociation = new List<GlAcctsMemos>() { new GlAcctsMemos()};
                dataReaderMock.Setup(acc => acc.ReadRecordAsync<DataContracts.GlAccts>(It.IsAny<string>(), It.IsAny<bool>()))
                   .ReturnsAsync(_glAcctsDataContract.FirstOrDefault(p => p.Recordkey == "11_00_01_00_20603_52010"));
                dataReaderMock.Setup(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
                {
                    var result = new Dictionary<string, GuidLookupResult>();
                    foreach (var gl in gla)
                    {
                        result.Add(gl.Guid, new GuidLookupResult() { Entity = "GL.ACCTS", PrimaryKey = "1" });
                    }
                    return Task.FromResult(result);
                });
                try
                {
                    await _referenceDataRepo.GetAccountingStringComponentValue2ByGuid(record.RecordGuid);
                }
                catch (RepositoryException e)
                {
                    Assert.AreEqual(1, e.Errors.Count());
                    Assert.AreEqual("The record associated to the accounting string component value contains an invalid element. guid: '4f937f08-f6a0-4a1c-8d55-9f2a6dd6be46'",
                        e.Errors[0].Message);
                }
            }

            [TestMethod]
            public async Task ColleagueFinanceReferenceData_GetAccountingStringComponentValueByGuid2_AvailFundsController()
            {
                var record = _glAcctsDataContract.FirstOrDefault(p => p.Recordkey == "11_00_01_00_20603_52010");
                record.MemosEntityAssociation = new List<GlAcctsMemos>() { new GlAcctsMemos() { AvailFundsControllerAssocMember = "2000" } };
                dataReaderMock.Setup(acc => acc.ReadRecordAsync<DataContracts.GlAccts>(It.IsAny<string>(), It.IsAny<bool>()))
                   .ReturnsAsync(_glAcctsDataContract.FirstOrDefault(p => p.Recordkey == "11_00_01_00_20603_52010"));
                dataReaderMock.Setup(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
                {
                    var result = new Dictionary<string, GuidLookupResult>();
                    foreach (var gl in gla)
                    {
                        result.Add(gl.Guid, new GuidLookupResult() { Entity = "GL.ACCTS", PrimaryKey = "1" });
                    }
                    return Task.FromResult(result);
                });
                try
                {
                    await _referenceDataRepo.GetAccountingStringComponentValue2ByGuid(record.RecordGuid);
                }
                catch (RepositoryException e)
                {
                    Assert.AreEqual(1, e.Errors.Count());
                    Assert.AreEqual("The record associated to the accounting string component value contains an invalid element. guid: '4f937f08-f6a0-4a1c-8d55-9f2a6dd6be46'",
                        e.Errors[0].Message);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task ColleagueFinanceReferenceData_GetAccountingStringComponentValue2ByGuid_KeyNotFound()
            {
                dataReaderMock.Setup(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
                {
                    var result = new Dictionary<string, GuidLookupResult>();
                    foreach (var gl in gla)
                    {
                        result.Add(gl.Guid, new GuidLookupResult() { Entity = "", PrimaryKey = "" });
                    }
                    return Task.FromResult(result);
                });
                await _referenceDataRepo.GetAccountingStringComponentValue2ByGuid(guid);

            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task ColleagueFinanceReferenceData_GetAccountingStringComponentValue2ByGuid_InvalidEntity()
            {
                dataReaderMock.Setup(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
                {
                    var result = new Dictionary<string, GuidLookupResult>();
                    foreach (var gl in gla)
                    {
                        result.Add(gl.Guid, new GuidLookupResult() { Entity = "INVALID", PrimaryKey = "1" });
                    }
                    return Task.FromResult(result);
                });
                await _referenceDataRepo.GetAccountingStringComponentValue2ByGuid(guid);
            }

            [TestMethod]
            public async Task ColleagueFinanceReferenceData_GetAccountingStringComponentValues2_Component_All_NonCache()
            {
                var actuals = await _referenceDataRepo.GetAccountingStringComponentValues2Async(offset, limit, "", "", "");
                Assert.IsNotNull(actuals);
                var actual = actuals.Item1.FirstOrDefault(a => a.Guid == guid);
                var expected = _glAcctsDataContract.FirstOrDefault(glAcct => glAcct.RecordGuid == guid);
                var expectedCc = _glAcctsCcDataContract.FirstOrDefault(cc => cc.Recordkey == expected.Recordkey);
                Assert.IsNotNull(actual);
                Assert.IsNotNull(expected);
                Assert.IsNotNull(expectedCc);
                Assert.AreEqual(guid, actual.Guid);
                Assert.AreEqual(expectedCc.GlccAcctDesc, actual.Description);
                Assert.AreEqual("available", actual.Status);
                Assert.AreEqual("liability", actual.Type);
                Assert.AreEqual("GL", actual.AccountDef);
                Assert.AreEqual(expected.Recordkey, actual.AccountNumber);
            }

            [TestMethod]
            public async Task ColleagueFinanceReferenceData_GetAccountingStringComponentValues2_Component_GlAcct_NonCache()
            {
                var actuals = await _referenceDataRepo.GetAccountingStringComponentValues2Async(offset, limit, "GL.ACCT", "", "");
                Assert.IsNotNull(actuals);
                var actual = actuals.Item1.FirstOrDefault(a => a.Guid == guid);
                var expected = _glAcctsDataContract.FirstOrDefault(glAcct => glAcct.RecordGuid == guid);
                var expectedCc = _glAcctsCcDataContract.FirstOrDefault(cc => cc.Recordkey == expected.Recordkey);
                Assert.IsNotNull(actual);
                Assert.IsNotNull(expected);
                Assert.IsNotNull(expectedCc);
                Assert.AreEqual(guid, actual.Guid);
                Assert.AreEqual(expectedCc.GlccAcctDesc, actual.Description);
                Assert.AreEqual("available", actual.Status);
                Assert.AreEqual("liability", actual.Type);
                Assert.AreEqual("GL", actual.AccountDef);
                Assert.AreEqual(expected.Recordkey, actual.AccountNumber);
            }

            [TestMethod]
            public async Task ColleagueFinanceReferenceData_GetAccountingStringComponentValues2_Component_Project_NonCache()
            {
                var actuals = await _referenceDataRepo.GetAccountingStringComponentValues2Async(offset, limit, "PROJECT", "", "");
                Assert.IsNotNull(actuals);
            }

            [TestMethod]
            public async Task ColleagueFinanceReferenceData_GetAccountingStringComponentValues2_Component_All_Cache()
            {
                var actuals = await _referenceDataRepo.GetAccountingStringComponentValues2Async(offset, limit, "", "", "");
                Assert.IsNotNull(actuals);
                var actual = actuals.Item1.FirstOrDefault(a => a.Guid == guid);
                var expected = _glAcctsDataContract.FirstOrDefault(glAcct => glAcct.RecordGuid == guid);
                var expectedCc = _glAcctsCcDataContract.FirstOrDefault(cc => cc.Recordkey == expected.Recordkey);
                Assert.IsNotNull(actual);
                Assert.IsNotNull(expected);
                Assert.IsNotNull(expectedCc);
                Assert.AreEqual(guid, actual.Guid);
                Assert.AreEqual(expectedCc.GlccAcctDesc, actual.Description);
                Assert.AreEqual("available", actual.Status);
                Assert.AreEqual("liability", actual.Type);
                Assert.AreEqual("GL", actual.AccountDef);
                Assert.AreEqual(expected.Recordkey, actual.AccountNumber);
            }

            [TestMethod]
            public async Task ColleagueFinanceReferenceData_GetAccountingStringComponentValues2_Component_GlAcct_Cache()
            {
                var actuals = await _referenceDataRepo.GetAccountingStringComponentValues2Async(offset, limit, "GL.ACCT", "", "");
                Assert.IsNotNull(actuals);
                var actual = actuals.Item1.FirstOrDefault(a => a.Guid == guid);
                var expected = _glAcctsDataContract.FirstOrDefault(glAcct => glAcct.RecordGuid == guid);
                var expectedCc = _glAcctsCcDataContract.FirstOrDefault(cc => cc.Recordkey == expected.Recordkey);
                Assert.IsNotNull(actual);
                Assert.IsNotNull(expected);
                Assert.IsNotNull(expectedCc);
                Assert.AreEqual(guid, actual.Guid);
                Assert.AreEqual(expectedCc.GlccAcctDesc, actual.Description);
                Assert.AreEqual("available", actual.Status);
                Assert.AreEqual("liability", actual.Type);
                Assert.AreEqual("GL", actual.AccountDef);
                Assert.AreEqual(expected.Recordkey, actual.AccountNumber);
            }

            [TestMethod]
            public async Task ColleagueFinanceReferenceData_GetAccountingStringComponentValues2_Component_Project_Cache()
            {
                var actuals = await _referenceDataRepo.GetAccountingStringComponentValues2Async(offset, limit, "PROJECT", "", "");
                Assert.IsNotNull(actuals);
            }

            [TestMethod]
            public async Task ColleagueFinanceReferenceData_GetAccountingStringComponentValues2_FiscalYear_Null()
            {
                _fiscalYrDataContract = null;
                var actuals = await _referenceDataRepo.GetAccountingStringComponentValues2Async(offset, limit, "PROJECT", "", "");
                Assert.IsNotNull(actuals);
            }

            [TestMethod]
            public async Task ColleagueFinanceReferenceData_GetAccountingStringComponentValues2_FiscalYear_NoQualifying()
            {
                _fiscalYrDataContract = new Fiscalyr()
                {
                    Recordkey = "1",
                    CfCurrentFiscalYear = "1999",
                    FiscalStartMonth = 1
                };
                var actuals = await _referenceDataRepo.GetAccountingStringComponentValues2Async(offset, limit, "PROJECT", "", "");
                Assert.IsNotNull(actuals);
            }

            [TestMethod]
            public async Task ColleagueFinanceReferenceData_GetAccountingStringComponentValues2_Project_Type()
            {
                _glClsdefDataContract = new Glclsdef()
                {
                    GlClassLocation = null
                };

                var actuals = await _referenceDataRepo.GetAccountingStringComponentValues2Async(offset, limit, "PROJECT", "", "");
                Assert.IsNotNull(actuals);
                foreach (var actual in actuals.Item1)
                {
                    Assert.AreEqual("expense", actual.Type);
                }
            }

            [TestMethod]
            public async Task ColleagueFinanceReferenceData_GetGuidsForPooleeGLAcctsInFiscalYearsAsync()
            {
                Dictionary<string, RecordKeyLookupResult> results = new Dictionary<string, RecordKeyLookupResult>();
                RecordKeyLookupResult recKeyResult = new RecordKeyLookupResult() { Guid = "1AA09265-F53F-4D68-85D6-BF8903362526", ModelName = "GL.ACCTS" };
                results.Add("GL.ACCTS+1", recKeyResult);
                dataReaderMock.Setup(i => i.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(results);
                var actual = await _referenceDataRepo.GetGuidsForPooleeGLAcctsInFiscalYearsAsync(new[] { "1" });
                Assert.IsNotNull(actual);
                Assert.AreEqual(actual.Keys.ToList()[0], "1");
                Assert.AreEqual(actual.Values.ToList()[0], "1AA09265-F53F-4D68-85D6-BF8903362526");
            }

            private void BuildData()
            {
                _glAcctsDataContract = new Collection<GlAccts>()
                {
                    new DataContracts.GlAccts()
                    {
                        Recordkey = "11_00_01_00_20603_52010",
                        AvailFundsController = new List<string>() {"2016", "2017"},
                        RecordGuid = guid,
                        MemosEntityAssociation = new List<DataContracts.GlAcctsMemos>()
                        {
                            new DataContracts.GlAcctsMemos()
                            {
                                AvailFundsControllerAssocMember = "2017",
                                GlFreezeFlagsAssocMember = "O",
                                GlBudgetPostedAssocMember = 100,
                                GlBudgetMemosAssocMember = 500,
                                GlActualPostedAssocMember = 10,
                                GlActualMemosAssocMember = 50,
                                GlEncumbrancePostedAssocMember = 20,
                                GlEncumbranceMemosAssocMember = 30,
                                GlRequisitionMemosAssocMember = 100                                
                            },
                            new DataContracts.GlAcctsMemos()
                            {
                                AvailFundsControllerAssocMember = "2016",
                                GlFreezeFlagsAssocMember = "C",
                                GlBudgetPostedAssocMember = 100,
                                GlBudgetMemosAssocMember = 500,
                                GlActualPostedAssocMember = 10,
                                GlActualMemosAssocMember = 50,
                                GlEncumbrancePostedAssocMember = 20,
                                GlEncumbranceMemosAssocMember = 30,
                                GlRequisitionMemosAssocMember = 100,
                            }
                        },
                        GlInactive = "A"

                    },
                    new DataContracts.GlAccts()
                    {
                        Recordkey = "11_00_01_00_20603_12010",
                        AvailFundsController = new List<string>() {"2017"},
                        RecordGuid = "3AA09265-F53F-4D68-85D6-BF8903362527",
                        MemosEntityAssociation = new List<DataContracts.GlAcctsMemos>()
                        {
                            new DataContracts.GlAcctsMemos()
                            {
                                AvailFundsControllerAssocMember = "2017",
                                GlFreezeFlagsAssocMember = "F",
                                GlBudgetPostedAssocMember = 100,
                                GlBudgetMemosAssocMember = 500,
                                GlActualPostedAssocMember = 10,
                                GlActualMemosAssocMember = 50,
                                GlEncumbrancePostedAssocMember = 20,
                                GlEncumbranceMemosAssocMember = 30,
                                GlRequisitionMemosAssocMember = 100,
                                GlPooledTypeAssocMember = "P"

                            }
                        }
                    }
                };

                _glClsdefDataContract = new Glclsdef()
                {
                    GlClassLocation = new List<int?>() { 19, 1 },
                    GlClassAssetValues = new List<string>() { "1" },
                    GlClassLiabilityValues = new List<string>() { "5" }
                };

                _glAcctsCcDataContract = new Collection<GlAcctsCc>()
                {
                    new GlAcctsCc()
                    {
                        GlccAcctDesc = "Description",
                        Recordkey = "11_00_01_00_20603_52010"
                    }
                };

                _fiscalYrDataContract = new Fiscalyr()
                {
                    Recordkey = "1",
                    CfCurrentFiscalYear = "2017",
                    FiscalStartMonth = 1
                };

                _projectsDataContract = new Collection<Projects>()
                {
                    new Projects() {RecordGuid = "A42A664D-70FC-4EE9-B31C-B57B652E719B", Recordkey = "1", PrjRefNo = "STPU-001", PrjType = "R", PrjCurrentStatus = "A", PrjTitle = "Test1"},
                    new Projects() {RecordGuid = "B42A664D-70FC-4EE9-B31C-B57B652E719B", Recordkey = "2", PrjRefNo = "STPU-002", PrjType = "A", PrjCurrentStatus = "X", PrjTitle = "Test2"},
                    new Projects() {RecordGuid = "C42A664D-70FC-4EE9-B31C-B57B652E719B", Recordkey = "3", PrjRefNo = "STPU-003", PrjType = "T", PrjCurrentStatus = "I", PrjTitle = "Test3"}

                };
            }
        }

        /// <summary>
        /// AccountingStringComponentValuesTests2
        /// </summary>
        [TestClass]
        public class AccountingStringComponentValuesTests3 : BaseRepositorySetup
        {
            private ColleagueFinanceReferenceDataRepository _referenceDataRepo;
            private Mock<ColleagueFinanceReferenceDataRepository> _referenceDataRepoMock;
            private Collection<DataContracts.GlAccts> _glAcctsDataContract;
            private DataContracts.Glclsdef _glClsdefDataContract;
            private Collection<DataContracts.GlAcctsCc> _glAcctsCcDataContract;
            private Fiscalyr _fiscalYrDataContract;
            private Collection<DataContracts.Projects> _projectsDataContract;


            private string[] ids = new string[] { "1", "2", "3", "4" };
            private string expectedRecordKey = "1";
            private string guid = "4f937f08-f6a0-4a1c-8d55-9f2a6dd6be46";
            private int offset = 0;
            private int limit = 4;

            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();
                _referenceDataRepo = new ColleagueFinanceReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
                _referenceDataRepoMock = new Mock<ColleagueFinanceReferenceDataRepository>();
                BuildData();

                var glIds = _glAcctsDataContract.Select(i => i.Recordkey).ToArray();
                var prIds = _projectsDataContract.Select(i => i.Recordkey).ToArray();

                dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.GlAccts>("GL.ACCTS", It.IsAny<string[]>(), It.IsAny<bool>()))
                    .ReturnsAsync(_glAcctsDataContract);
                dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.GlAccts>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                    .ReturnsAsync(_glAcctsDataContract);

                dataReaderMock.Setup(acc => acc.ReadRecordAsync<DataContracts.Glclsdef>("ACCOUNT.PARAMETERS", "GL.CLASS.DEF", It.IsAny<bool>()))
                    .ReturnsAsync(_glClsdefDataContract);

                dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.GlAcctsCc>("GL.ACCTS.CC", "", It.IsAny<bool>()))
                    .ReturnsAsync(_glAcctsCcDataContract);

                dataReaderMock.Setup(acc => acc.ReadRecordAsync<DataContracts.Fiscalyr>("ACCOUNT.PARAMETERS", "FISCAL.YEAR", It.IsAny<bool>()))
                    .ReturnsAsync(_fiscalYrDataContract);

                dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.Projects>("PROJECTS", It.IsAny<string[]>(), It.IsAny<bool>()))
                    .ReturnsAsync(_projectsDataContract);
                dataReaderMock.Setup(acc => acc.SelectAsync("PROJECTS", string.Empty)).ReturnsAsync(prIds);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                    x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .Returns(Task.FromResult(new Tuple<object, SemaphoreSlim>(
                        null,
                        new SemaphoreSlim(1, 1)
                        )));
            }

            [TestCleanup]
            public void Cleanup()
            {
                MockCleanup();
                _referenceDataRepo = null;
                _referenceDataRepoMock = null;
            }


            [TestMethod]
            public async Task ColleagueFinanceReferenceData_GetAccountingStringComponentValue3ByGuid_Projects()
            {
                var projectGuid = "A42A664D-70FC-4EE9-B31C-B57B652E719B";
                dataReaderMock.Setup(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
                {
                    var result = new Dictionary<string, GuidLookupResult>();
                    foreach (var gl in gla)
                    {
                        result.Add(gl.Guid, new GuidLookupResult() { Entity = "PROJECTS", PrimaryKey = "1" });
                    }
                    return Task.FromResult(result);
                });

                dataReaderMock.Setup(acc => acc.ReadRecordAsync<DataContracts.Projects>(It.IsAny<string>(), It.IsAny<bool>()))
                  .ReturnsAsync(_projectsDataContract.FirstOrDefault(p => p.Recordkey == "1"));

                var actuals = await _referenceDataRepo.GetAccountingStringComponentValue3ByGuid(projectGuid);
                Assert.IsNotNull(actuals);
            }

            [TestMethod]
            public async Task ColleagueFinanceReferenceData_GetAccountingStringComponentValue3ByGuid_GlAccts()
            {
                dataReaderMock.Setup(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
                {
                    var result = new Dictionary<string, GuidLookupResult>();
                    foreach (var gl in gla)
                    {
                        result.Add(gl.Guid, new GuidLookupResult() { Entity = "GL.ACCTS", PrimaryKey = "11_00_01_00_20603_52010" });
                    }
                    return Task.FromResult(result);
                });

                dataReaderMock.Setup(acc => acc.ReadRecordAsync<DataContracts.GlAccts>(It.IsAny<string>(), It.IsAny<bool>()))
                    .ReturnsAsync(_glAcctsDataContract.FirstOrDefault(p => p.Recordkey == "11_00_01_00_20603_52010"));

                var actuals = await _referenceDataRepo.GetAccountingStringComponentValue3ByGuid(guid);
                Assert.IsNotNull(actuals);
            }

            [TestMethod]
            public async Task ColleagueFinanceReferenceData_GetAccountingStringComponentValues3_Component_Project_Cache()
            {
                var actuals = await _referenceDataRepo.GetAccountingStringComponentValues3Async(offset, limit, "PROJECT", "", "", new List<string>(), null);
                Assert.IsNotNull(actuals);
            }

            [TestMethod]
            public async Task ColleagueFinanceReferenceData_GetAccountingStringComponentValues3_Component_Project_NonCache()
            {
                var actuals = await _referenceDataRepo.GetAccountingStringComponentValues3Async(offset, limit, "PROJECT", "", "", new List<string>(), null);
                Assert.IsNotNull(actuals);
            }

            [TestMethod]
            public async Task ColleagueFinanceReferenceData_GetAccountingStringComponentValues3_Component_GlAcct_NonCache()
            {
                var actuals = await _referenceDataRepo.GetAccountingStringComponentValues3Async(offset, limit, "GL.ACCT", "", "", new List<string>(), null);
                Assert.IsNotNull(actuals);
            }

            [TestMethod]
            public async Task ColleagueFinanceReferenceData_GetAccountingStringComponentValues3_Component_GlAcct_Cache()
            {
                var actuals = await _referenceDataRepo.GetAccountingStringComponentValues3Async(offset, limit, "GL.ACCT", "", "", new List<string>(), null);
                Assert.IsNotNull(actuals);
            }

            [TestMethod]
            public async Task ColleagueFinanceReferenceData_GetAccountingStringComponentValues3_Component_Default_Cache()
            {
                var actuals = await _referenceDataRepo.GetAccountingStringComponentValues3Async(offset, limit, "", "", "", new List<string>(), null);
                Assert.IsNotNull(actuals);
            }

            [TestMethod]
            public async Task ColleagueFinanceReferenceData_GetAccountingStringComponentValues3_Component_Default_NonCache()
            {
                var actuals = await _referenceDataRepo.GetAccountingStringComponentValues3Async(offset, limit, "", "", "", new List<string>(), null);
                Assert.IsNotNull(actuals);
            }

            [TestMethod]
            public async Task ColleagueFinanceReferenceData_GetAccountingStringComponentValues3_Component_Status_Available()
            {
                var actuals = await _referenceDataRepo.GetAccountingStringComponentValues3Async(offset, limit, "", "", "available", new List<string>(), null);
                Assert.IsNotNull(actuals);
            }

            [TestMethod]
            public async Task ColleagueFinanceReferenceData_GetAccountingStringComponentValues3_Component_Grants()
            {
                var actuals = await _referenceDataRepo.GetAccountingStringComponentValues3Async(offset, limit, "", "", "available", new List<string>() { "1" }, null);
                Assert.IsNotNull(actuals);
            }

            [TestMethod]
            public async Task ColleagueFinanceReferenceData_GetAccountingStringComponentValues3_Component_EffectiveOn()
            {
                DateTime? date = new DateTime(2017, 5, 1);
                var actuals = await _referenceDataRepo.GetAccountingStringComponentValues3Async(offset, limit, "", "", "available", new List<string>() { "1" }, date);
                Assert.IsNotNull(actuals);
            }

            [TestMethod]
            public async Task ColleagueFinanceReferenceData_GetAccountingStringComponentValues3_Component_WithTyeAccount()
            {
                DateTime? date = new DateTime(2017, 5, 1);
                var actuals = await _referenceDataRepo.GetAccountingStringComponentValues3Async(offset, limit, "", "asset", "available", new List<string>() { "1" }, date);
                Assert.IsNotNull(actuals);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task ColleagueFinanceReferenceData_GetAccountingStringComponentValueByGuid3_AccountingComponent()
            {
                var record = _glAcctsDataContract.FirstOrDefault(p => p.Recordkey == "11_00_01_00_20603_52010");
                record.MemosEntityAssociation = new List<GlAcctsMemos>() { new GlAcctsMemos()};
                dataReaderMock.Setup(acc => acc.ReadRecordAsync<DataContracts.GlAccts>(It.IsAny<string>(), It.IsAny<bool>()))
                   .ReturnsAsync(_glAcctsDataContract.FirstOrDefault(p => p.Recordkey == "11_00_01_00_20603_52010"));
                dataReaderMock.Setup(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
                {
                    var result = new Dictionary<string, GuidLookupResult>();
                    foreach (var gl in gla)
                    {
                        result.Add(gl.Guid, new GuidLookupResult() { Entity = "GL.ACCTS", PrimaryKey = "1" });
                    }
                    return Task.FromResult(result);
                });
                try
                {
                    await _referenceDataRepo.GetAccountingStringComponentValue3ByGuid(record.RecordGuid);
                }
                catch (RepositoryException e)
                {
                    Assert.AreEqual(1, e.Errors.Count());
                    Assert.AreEqual("The record associated to the accounting string component value contains an invalid element. guid: '4f937f08-f6a0-4a1c-8d55-9f2a6dd6be46'",
                        e.Errors[0].Message);
                    throw e;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task ColleagueFinanceReferenceData_GetAccountingStringComponentValueByGuid3_AvailFundsController()
            {
                var record = _glAcctsDataContract.FirstOrDefault(p => p.Recordkey == "11_00_01_00_20603_52010");
                record.MemosEntityAssociation = new List<GlAcctsMemos>() { new GlAcctsMemos() { AvailFundsControllerAssocMember = "2000" } };
                dataReaderMock.Setup(acc => acc.ReadRecordAsync<DataContracts.GlAccts>(It.IsAny<string>(), It.IsAny<bool>()))
                   .ReturnsAsync(_glAcctsDataContract.FirstOrDefault(p => p.Recordkey == "11_00_01_00_20603_52010"));
                dataReaderMock.Setup(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
                {
                    var result = new Dictionary<string, GuidLookupResult>();
                    foreach (var gl in gla)
                    {
                        result.Add(gl.Guid, new GuidLookupResult() { Entity = "GL.ACCTS", PrimaryKey = "1" });
                    }
                    return Task.FromResult(result);
                });
                try
                {
                    await _referenceDataRepo.GetAccountingStringComponentValue3ByGuid(record.RecordGuid);
                }
                catch (RepositoryException e)
                {
                    Assert.AreEqual(1, e.Errors.Count());
                    Assert.AreEqual("The record associated to the accounting string component value contains an invalid element. guid: '4f937f08-f6a0-4a1c-8d55-9f2a6dd6be46'",
                        e.Errors[0].Message);
                    throw e;
                }
            }

            [TestMethod]
            public async Task GetAccountingStringComponentValues3_WithTransactionStatus_EffectiveOn()
            {
                DateTime? date = new DateTime(2017, 5, 1);

                var glIds = _glAcctsDataContract.Select(i => i.Recordkey).ToArray();
                var prIds = _projectsDataContract.Select(i => i.Recordkey).ToArray();

                dataReaderMock.Setup(acc => acc.SelectAsync("GL.ACCTS", It.IsAny<string>())).ReturnsAsync(glIds);
                dataReaderMock.Setup(acc => acc.SelectAsync("GL.ACCTS", glIds, It.IsAny<string>())).ReturnsAsync(glIds);
                //dataReaderMock.Setup(acc => acc.SelectAsync("GL.ACCTS", glIds, It.IsAny<string>())).ReturnsAsync(glIds);
                dataReaderMock.Setup(acc => acc.SelectAsync("PROJECTS", It.IsAny<string>())).ReturnsAsync(prIds);
                dataReaderMock.Setup(acc => acc.SelectAsync("PROJECTS", prIds, It.IsAny<string>())).ReturnsAsync(prIds);

                var actuals = await _referenceDataRepo.GetAccountingStringComponentValues3Async(offset, limit, "", "", "available", null, date);
                Assert.IsNotNull(actuals);
                Assert.AreEqual(5, actuals.Item2);
            }

            [TestMethod]
            public async Task ColleagueFinanceReferenceData_GetAccountingStringComponentValues3_Component_AssetTypeAccount()
            {
                var glIds = _glAcctsDataContract.Select(i => i.Recordkey).ToArray();
                var prIds = _projectsDataContract.Select(i => i.Recordkey).ToArray();
                dataReaderMock.Setup(acc => acc.SelectAsync("GL.ACCTS", string.Empty)).ReturnsAsync(glIds);
                dataReaderMock.Setup(acc => acc.SelectAsync("PROJECTS", string.Empty)).ReturnsAsync(prIds);

                var actuals = await _referenceDataRepo.GetAccountingStringComponentValues3Async(offset, limit, "", "asset", "", null, null);
                Assert.IsNotNull(actuals);
                Assert.AreEqual(1, actuals.Item2);
            }

            [TestMethod]
            public async Task ColleagueFinanceReferenceData_GetAccountingStringComponentValues3_Component_LiabilityTypeAccount()
            {
                var glIds = _glAcctsDataContract.Select(i => i.Recordkey).ToArray();
                var prIds = _projectsDataContract.Select(i => i.Recordkey).ToArray();
                dataReaderMock.Setup(acc => acc.SelectAsync("GL.ACCTS", string.Empty)).ReturnsAsync(glIds);
                dataReaderMock.Setup(acc => acc.SelectAsync("PROJECTS", string.Empty)).ReturnsAsync(prIds);

                var actuals = await _referenceDataRepo.GetAccountingStringComponentValues3Async(offset, limit, "", "liability", "", null, null);

                Assert.IsNotNull(actuals);
                Assert.AreEqual(0, actuals.Item2);
            }

            [TestMethod]
            public async Task ColleagueFinanceReferenceData_GetAccountingStringComponentValues3_Component_revenueTypeAccount()
            {
                var glIds = _glAcctsDataContract.Select(i => i.Recordkey).ToArray();
                var prIds = _projectsDataContract.Select(i => i.Recordkey).ToArray();
                dataReaderMock.Setup(acc => acc.SelectAsync("GL.ACCTS", string.Empty)).ReturnsAsync(glIds);
                dataReaderMock.Setup(acc => acc.SelectAsync("PROJECTS", string.Empty)).ReturnsAsync(prIds);

                var actuals = await _referenceDataRepo.GetAccountingStringComponentValues3Async(offset, limit, "", "revenue", "", null, null);

                Assert.IsNotNull(actuals);
                Assert.AreEqual(0, actuals.Item2);
            }

            [TestMethod]
            public async Task ColleagueFinanceReferenceData_GetAccountingStringComponentValues3_Component_FundBalanceTypeAccount()
            {
                var glIds = _glAcctsDataContract.Select(i => i.Recordkey).ToArray();
                var prIds = _projectsDataContract.Select(i => i.Recordkey).ToArray();
                dataReaderMock.Setup(acc => acc.SelectAsync("GL.ACCTS", string.Empty)).ReturnsAsync(glIds);
                dataReaderMock.Setup(acc => acc.SelectAsync("PROJECTS", string.Empty)).ReturnsAsync(prIds);

                var actuals = await _referenceDataRepo.GetAccountingStringComponentValues3Async(offset, limit, "", "fundBalance", "", null, null);

                Assert.IsNotNull(actuals);
                Assert.AreEqual(0, actuals.Item2);
            }

            [TestMethod]
            public async Task ColleagueFinanceReferenceData_GetAccountingStringComponentValues3_Component_ExpenseTypeAccount()
            {
                var glIds = _glAcctsDataContract.Select(i => i.Recordkey).ToArray();
                var prIds = _projectsDataContract.Select(i => i.Recordkey).ToArray();
                dataReaderMock.Setup(acc => acc.SelectAsync("GL.ACCTS", string.Empty)).ReturnsAsync(glIds);
                dataReaderMock.Setup(acc => acc.SelectAsync("PROJECTS", string.Empty)).ReturnsAsync(prIds);

                var actuals = await _referenceDataRepo.GetAccountingStringComponentValues3Async(offset, limit, "", "expense", "", null, null);

                Assert.IsNotNull(actuals);
                Assert.AreEqual(4, actuals.Item2);
            }

            [TestMethod]
            public async Task ColleagueFinanceReferenceData_GetAccountingStringComponentValues3_Component_NonPersonalExpenseTypeAccount()
            {
                var glIds = _glAcctsDataContract.Select(i => i.Recordkey).ToArray();
                var prIds = _projectsDataContract.Select(i => i.Recordkey).ToArray();
                dataReaderMock.Setup(acc => acc.SelectAsync("GL.ACCTS", string.Empty)).ReturnsAsync(glIds);
                dataReaderMock.Setup(acc => acc.SelectAsync("PROJECTS", string.Empty)).ReturnsAsync(prIds);

                var actuals = await _referenceDataRepo.GetAccountingStringComponentValues3Async(offset, limit, "", "nonPersonalExpense", "", null, null);

                Assert.IsNotNull(actuals);
                Assert.AreEqual(0, actuals.Item2);
            }

            [TestMethod]
            public async Task ColleagueFinanceReferenceData_GetAccountingStringComponentValues3_Grants_NoRecords()
            {
                var glIds = _glAcctsDataContract.Select(i => i.Recordkey).ToArray();
                var prIds = _projectsDataContract.Select(i => i.Recordkey).ToArray();
                dataReaderMock.Setup(acc => acc.SelectAsync("GL.ACCTS", string.Empty)).ReturnsAsync(glIds);
                dataReaderMock.Setup(acc => acc.SelectAsync("PROJECTS", string.Empty)).ReturnsAsync(prIds);

                var actuals = await _referenceDataRepo.GetAccountingStringComponentValues3Async(offset, limit, "", "", "", new List<string>() { "1" }, null);

                Assert.IsNotNull(actuals);
                Assert.AreEqual(0, actuals.Item2);
            }

            [TestMethod]
            public async Task ColleagueFinanceReferenceData_GetAccountingStringComponentValues3_Grants()
            {
                var glIds = _glAcctsDataContract.Select(i => i.Recordkey).ToArray();
                var prIds = _projectsDataContract.Select(i => i.Recordkey).ToArray();
                dataReaderMock.Setup(acc => acc.SelectAsync("GL.ACCTS", string.Empty)).ReturnsAsync(glIds);
                dataReaderMock.Setup(acc => acc.SelectAsync("GL.ACCTS", It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>())).ReturnsAsync(glIds);
                dataReaderMock.Setup(acc => acc.SelectAsync("PROJECTS", string.Empty)).ReturnsAsync(prIds);
                dataReaderMock.Setup(acc => acc.SelectAsync("PROJECTS", It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>())).ReturnsAsync(prIds);

                var actuals = await _referenceDataRepo.GetAccountingStringComponentValues3Async(offset, limit, "", "", "", new List<string>() { "1" }, null);

                Assert.IsNotNull(actuals);
                Assert.AreEqual(5, actuals.Item2);
            }

            [TestMethod]
            public async Task ColleagueFinanceReferenceData_GetAccountingStringComponentValues3_GL_ACCT()
            {
                var glIds = _glAcctsDataContract.Select(i => i.Recordkey).ToArray();
                dataReaderMock.Setup(acc => acc.SelectAsync("GL.ACCTS", string.Empty)).ReturnsAsync(glIds);
                dataReaderMock.Setup(acc => acc.SelectAsync("PROJECTS", string.Empty)).ReturnsAsync(null);

                var actuals = await _referenceDataRepo.GetAccountingStringComponentValues3Async(offset, limit, "GL.ACCT", "", "", null, null);

                Assert.IsNotNull(actuals);
                Assert.AreEqual(2, actuals.Item2);
            }

            [TestMethod]
            public async Task ColleagueFinanceReferenceData_GetAccountingStringComponentValues3_PROJECT()
            {
                var prIds = _projectsDataContract.Select(i => i.Recordkey).ToArray();
                dataReaderMock.Setup(acc => acc.SelectAsync("GL.ACCTS", string.Empty)).ReturnsAsync(null);
                dataReaderMock.Setup(acc => acc.SelectAsync("PROJECTS", string.Empty)).ReturnsAsync(prIds);

                var actuals = await _referenceDataRepo.GetAccountingStringComponentValues3Async(offset, limit, "PROJECT", "", "", null, null);

                Assert.IsNotNull(actuals);
                Assert.AreEqual(3, actuals.Item2);
            }

            private void BuildData()
            {
                _glAcctsDataContract = new Collection<GlAccts>()
                {
                    new DataContracts.GlAccts()
                    {
                        Recordkey = "11_00_01_00_20603_52010",
                        AvailFundsController = new List<string>() {"2016", "2017"},
                        RecordGuid = guid,
                        GlAcctsAddDate = new DateTime(2017, 5, 1),
                        MemosEntityAssociation = new List<DataContracts.GlAcctsMemos>()
                        {
                            new DataContracts.GlAcctsMemos()
                            {
                                AvailFundsControllerAssocMember = "2017",
                                GlFreezeFlagsAssocMember = "O",
                                GlBudgetPostedAssocMember = 100,
                                GlBudgetMemosAssocMember = 500,
                                GlActualPostedAssocMember = 10,
                                GlActualMemosAssocMember = 50,
                                GlEncumbrancePostedAssocMember = 20,
                                GlEncumbranceMemosAssocMember = 30,
                                GlRequisitionMemosAssocMember = 100                                
                            },
                            new DataContracts.GlAcctsMemos()
                            {
                                AvailFundsControllerAssocMember = "2016",
                                GlFreezeFlagsAssocMember = "C",
                                GlBudgetPostedAssocMember = 100,
                                GlBudgetMemosAssocMember = 500,
                                GlActualPostedAssocMember = 10,
                                GlActualMemosAssocMember = 50,
                                GlEncumbrancePostedAssocMember = 20,
                                GlEncumbranceMemosAssocMember = 30,
                                GlRequisitionMemosAssocMember = 100,
                            }
                        },
                        GlInactive = "A",
                        GlProjectsId = new List<string>() { "1" }

                    },
                    new DataContracts.GlAccts()
                    {
                        Recordkey = "11_00_01_00_20603_12010",
                        AvailFundsController = new List<string>() {"2017"},
                        RecordGuid = "3AA09265-F53F-4D68-85D6-BF8903362527",
                        MemosEntityAssociation = new List<DataContracts.GlAcctsMemos>()
                        {
                            new DataContracts.GlAcctsMemos()
                            {
                                AvailFundsControllerAssocMember = "2017",
                                GlFreezeFlagsAssocMember = "F",
                                GlBudgetPostedAssocMember = 100,
                                GlBudgetMemosAssocMember = 500,
                                GlActualPostedAssocMember = 10,
                                GlActualMemosAssocMember = 50,
                                GlEncumbrancePostedAssocMember = 20,
                                GlEncumbranceMemosAssocMember = 30,
                                GlRequisitionMemosAssocMember = 100,
                                GlPooledTypeAssocMember = "P"

                            }
                        }
                    }
                };

                _glClsdefDataContract = new Glclsdef()
                {
                    GlClassLocation = new List<int?>() { 19, 1 },
                    GlClassAssetValues = new List<string>() { "1" },
                    GlClassLiabilityValues = new List<string>() { "2" },
                    GlClassFundBalValues = new List<string>() { "3" },
                    GlClassRevenueValues = new List<string>() { "4" },
                    GlClassExpenseValues = new List<string>() { "5" }
                };

                _glAcctsCcDataContract = new Collection<GlAcctsCc>()
                {
                    new GlAcctsCc()
                    {
                        GlccAcctDesc = "Description",
                        Recordkey = "11_00_01_00_20603_52010"
                    },
                    new GlAcctsCc()
                    {
                        GlccAcctDesc = "Description",
                        Recordkey = "11_00_01_00_20603_12010"
                    }
                };

                _fiscalYrDataContract = new Fiscalyr()
                {
                    Recordkey = "1",
                    CfCurrentFiscalYear = "2017",
                    FiscalStartMonth = 1
                };

                _projectsDataContract = new Collection<Projects>()
                {
                    new Projects() {RecordGuid = "A42A664D-70FC-4EE9-B31C-B57B652E719B", Recordkey = "1", PrjRefNo = "STPU-001", PrjType = "R", PrjCurrentStatus = "A", PrjTitle = "Test1"},
                    new Projects() {RecordGuid = "B42A664D-70FC-4EE9-B31C-B57B652E719B", Recordkey = "2", PrjRefNo = "STPU-002", PrjType = "A", PrjCurrentStatus = "X", PrjTitle = "Test2"},
                    new Projects() {RecordGuid = "C42A664D-70FC-4EE9-B31C-B57B652E719B", Recordkey = "3", PrjRefNo = "STPU-003", PrjType = "T", PrjCurrentStatus = "I", PrjTitle = "Test3"}

                };
            }
        }

        /// <summary>
        /// Test class for Commodity Codes codes
        /// </summary>
        [TestClass]
        public class CommodityCodesTests
        {
            private Mock<IColleagueTransactionFactory> transFactoryMock;
            private Mock<ICacheProvider> cacheProviderMock;
            private Mock<IColleagueDataReader> dataAccessorMock;
            private Mock<ILogger> loggerMock;
            private IEnumerable<CommodityCode> allCommodityCodes;
            private string codeItemName;

            private ColleagueFinanceReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build responses used for mocking
                allCommodityCodes = new TestColleagueFinanceReferenceDataRepository().GetCommodityCodesAsync(false).Result;

                // Build repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                codeItemName = referenceDataRepo.BuildFullCacheKey("AllCommodityCodes");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                allCommodityCodes = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task GetsCommodityCodesCacheAsync()
            {
                var commodityCodes = await referenceDataRepo.GetCommodityCodesAsync(false);

                for (int i = 0; i < allCommodityCodes.Count(); i++)
                {
                    Assert.AreEqual(allCommodityCodes.ElementAt(i).Guid, commodityCodes.ElementAt(i).Guid);
                    Assert.AreEqual(allCommodityCodes.ElementAt(i).Code, commodityCodes.ElementAt(i).Code);
                    Assert.AreEqual(allCommodityCodes.ElementAt(i).Description, commodityCodes.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetsCommodityCodesNonCacheAsync()
            {
                var commodityCodes = await referenceDataRepo.GetCommodityCodesAsync(true);

                for (int i = 0; i < allCommodityCodes.Count(); i++)
                {
                    Assert.AreEqual(allCommodityCodes.ElementAt(i).Guid, commodityCodes.ElementAt(i).Guid);
                    Assert.AreEqual(allCommodityCodes.ElementAt(i).Code, commodityCodes.ElementAt(i).Code);
                    Assert.AreEqual(allCommodityCodes.ElementAt(i).Description, commodityCodes.ElementAt(i).Description);
                }
            }

            private ColleagueFinanceReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to CommodityCodes read
                var commoditiesCollection = new Collection<CommodityCodes>(allCommodityCodes.Select(record =>
                    new Data.ColleagueFinance.DataContracts.CommodityCodes()
                    {
                        Recordkey = record.Code,
                        CmdtyDesc = record.Description,
                        RecordGuid = record.Guid
                    }).ToList());

                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<CommodityCodes>("COMMODITY.CODES", "", true))
                    .ReturnsAsync(commoditiesCollection);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));


                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var commodity = allCommodityCodes.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] {"COMMODITY.CODES", commodity.Code}),
                            new RecordKeyLookupResult() {Guid = commodity.Guid});
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new ColleagueFinanceReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

                return referenceDataRepo;
            }
        }

        /// <summary>
        /// Test class for Commodity Unit Types codes
        /// </summary>
        [TestClass]
        public class CommodityUnitTypesTests
        {
            private Mock<IColleagueTransactionFactory> transFactoryMock;
            private Mock<ICacheProvider> cacheProviderMock;
            private Mock<IColleagueDataReader> dataAccessorMock;
            private Mock<ILogger> loggerMock;
            private IEnumerable<CommodityUnitType> allCommodityUnitTypes;
            private string codeItemName;

            private ColleagueFinanceReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build responses used for mocking
                allCommodityUnitTypes = new TestColleagueFinanceReferenceDataRepository().GetCommodityUnitTypesAsync(false).Result;

                // Build repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                codeItemName = referenceDataRepo.BuildFullCacheKey("AllCommodityUnitTypes");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                allCommodityUnitTypes = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task GetsCommodityUnitTypesCacheAsync()
            {
                var commodityCodes = await referenceDataRepo.GetCommodityUnitTypesAsync(false);

                for (int i = 0; i < allCommodityUnitTypes.Count(); i++)
                {
                    Assert.AreEqual(allCommodityUnitTypes.ElementAt(i).Guid, commodityCodes.ElementAt(i).Guid);
                    Assert.AreEqual(allCommodityUnitTypes.ElementAt(i).Code, commodityCodes.ElementAt(i).Code);
                    Assert.AreEqual(allCommodityUnitTypes.ElementAt(i).Description, commodityCodes.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetsCommodityUnitTypesNonCacheAsync()
            {
                var commodityCodes = await referenceDataRepo.GetCommodityUnitTypesAsync(true);

                for (int i = 0; i < allCommodityUnitTypes.Count(); i++)
                {
                    Assert.AreEqual(allCommodityUnitTypes.ElementAt(i).Guid, commodityCodes.ElementAt(i).Guid);
                    Assert.AreEqual(allCommodityUnitTypes.ElementAt(i).Code, commodityCodes.ElementAt(i).Code);
                    Assert.AreEqual(allCommodityUnitTypes.ElementAt(i).Description, commodityCodes.ElementAt(i).Description);
                }
            }

            private ColleagueFinanceReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to CommodityUnitTypes read
                var commoditiesCollection = new Collection<UnitIssues>(allCommodityUnitTypes.Select(record =>
                    new Data.ColleagueFinance.DataContracts.UnitIssues()
                    {
                        Recordkey = record.Code,
                        UiDesc = record.Description,
                        RecordGuid = record.Guid
                    }).ToList());

                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<UnitIssues>("UNIT.ISSUES", "", true))
                    .ReturnsAsync(commoditiesCollection);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));


                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var commodity = allCommodityUnitTypes.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] {"UNIT.ISSUES", commodity.Code}),
                            new RecordKeyLookupResult() {Guid = commodity.Guid});
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new ColleagueFinanceReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

                return referenceDataRepo;
            }
        }

        /// <summary>
        /// Test class for FreeOnBoardType codes
        /// </summary>
        [TestClass]
        public class FreeOnBoardTypeTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<FreeOnBoardType> _freeOnBoardTypesCollection;
            string codeItemName;

            ColleagueFinanceReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build responses used for mocking
                _freeOnBoardTypesCollection = new List<FreeOnBoardType>()
                {
                    new FreeOnBoardType("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new FreeOnBoardType("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new FreeOnBoardType("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

                // Build repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                codeItemName = referenceDataRepo.BuildFullCacheKey("AllFreeOnBoardTypes");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                _freeOnBoardTypesCollection = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task GetsFreeOnBoardTypesCacheAsync()
            {
                var result = await referenceDataRepo.GetFreeOnBoardTypesAsync(false);

                for (int i = 0; i < _freeOnBoardTypesCollection.Count(); i++)
                {
                    Assert.AreEqual(_freeOnBoardTypesCollection.ElementAt(i).Guid, result.ElementAt(i).Guid);
                    Assert.AreEqual(_freeOnBoardTypesCollection.ElementAt(i).Code, result.ElementAt(i).Code);
                    Assert.AreEqual(_freeOnBoardTypesCollection.ElementAt(i).Description, result.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetsFreeOnBoardTypesNonCacheAsync()
            {
                var result = await referenceDataRepo.GetFreeOnBoardTypesAsync(true);

                for (int i = 0; i < _freeOnBoardTypesCollection.Count(); i++)
                {
                    Assert.AreEqual(_freeOnBoardTypesCollection.ElementAt(i).Guid, result.ElementAt(i).Guid);
                    Assert.AreEqual(_freeOnBoardTypesCollection.ElementAt(i).Code, result.ElementAt(i).Code);
                    Assert.AreEqual(_freeOnBoardTypesCollection.ElementAt(i).Description, result.ElementAt(i).Description);
                }
            }

            private ColleagueFinanceReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to Fobs read
                var entityCollection = new Collection<Fobs>(_freeOnBoardTypesCollection.Select(record =>
                    new Data.ColleagueFinance.DataContracts.Fobs()
                    {
                        Recordkey = record.Code,
                        FobsDesc = record.Description,
                        RecordGuid = record.Guid
                    }).ToList());

                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<Fobs>("FOBS", "", true))
                    .ReturnsAsync(entityCollection);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));


                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var entity = _freeOnBoardTypesCollection.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "FOBS", entity.Code }),
                            new RecordKeyLookupResult() { Guid = entity.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new ColleagueFinanceReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

                return referenceDataRepo;
            }
        }

        /// <summary>
        /// Test class for ShippingMethod codes
        /// </summary>
        [TestClass]
        public class ShippingMethodTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<ShippingMethod> _shippingMethodsCollection;
            IEnumerable<ShipViaCode> _shipViaCodesCollection;
            string codeItemName;

            ColleagueFinanceReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build responses used for mocking
                _shippingMethodsCollection = new List<ShippingMethod>()
                {
                    new ShippingMethod("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new ShippingMethod("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new ShippingMethod("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

                _shipViaCodesCollection = new List<ShipViaCode>()
                {
                    new ShipViaCode("MC", "Main Campus"),
                    new ShipViaCode("EC", "East Campus"),
                    new ShipViaCode("SC", "South Campus")
                };

                // Build repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                codeItemName = referenceDataRepo.BuildFullCacheKey("AllShippingMethods");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                _shippingMethodsCollection = null;
                referenceDataRepo = null;
                _shipViaCodesCollection = null;
            }

            [TestMethod]
            public async Task GetShipViaCodesAsync()
            {
                // Setup response to ShipToCodes read
                var entityCollection = new Collection<ShipVias>(_shipViaCodesCollection.Select(record =>
                    new Data.ColleagueFinance.DataContracts.ShipVias()
                    {
                        Recordkey = record.Code,
                        ShipViasDesc = record.Description,
                    }).ToList());

                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<ShipVias>("SHIP.VIAS", "", true))
                    .ReturnsAsync(entityCollection);

                var result = await referenceDataRepo.GetShipViaCodesAsync();

                for (int i = 0; i < _shipViaCodesCollection.Count(); i++)
                {
                    Assert.AreEqual(_shipViaCodesCollection.ElementAt(i).Code, result.ElementAt(i).Code);
                    Assert.AreEqual(_shipViaCodesCollection.ElementAt(i).Description, result.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetsShippingMethodsCacheAsync()
            {
                var result = await referenceDataRepo.GetShippingMethodsAsync(false);

                for (int i = 0; i < _shippingMethodsCollection.Count(); i++)
                {
                    Assert.AreEqual(_shippingMethodsCollection.ElementAt(i).Guid, result.ElementAt(i).Guid);
                    Assert.AreEqual(_shippingMethodsCollection.ElementAt(i).Code, result.ElementAt(i).Code);
                    Assert.AreEqual(_shippingMethodsCollection.ElementAt(i).Description, result.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetsShippingMethodsNonCacheAsync()
            {
                var result = await referenceDataRepo.GetShippingMethodsAsync(true);

                for (int i = 0; i < _shippingMethodsCollection.Count(); i++)
                {
                    Assert.AreEqual(_shippingMethodsCollection.ElementAt(i).Guid, result.ElementAt(i).Guid);
                    Assert.AreEqual(_shippingMethodsCollection.ElementAt(i).Code, result.ElementAt(i).Code);
                    Assert.AreEqual(_shippingMethodsCollection.ElementAt(i).Description, result.ElementAt(i).Description);
                }
            }

            private ColleagueFinanceReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to Fobs read
                var entityCollection = new Collection<ShipVias>(_shippingMethodsCollection.Select(record =>
                    new Data.ColleagueFinance.DataContracts.ShipVias()
                    {
                        Recordkey = record.Code,
                        ShipViasDesc = record.Description,
                        RecordGuid = record.Guid
                    }).ToList());

                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<ShipVias>("SHIP.VIAS", "", true))
                    .ReturnsAsync(entityCollection);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));


                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var entity = _shippingMethodsCollection.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "SHIP.VIAS", entity.Code }),
                            new RecordKeyLookupResult() { Guid = entity.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new ColleagueFinanceReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

                return referenceDataRepo;
            }
        }

        /// <summary>
        /// Test class for ShipToDestination codes
        /// </summary>
        [TestClass]
        public class ShipToDestinationTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<ShipToDestination> _shipToDestinationsCollection;
            IEnumerable<ShipToCode> _shipToCodesCollection;
            string codeItemName;

            ColleagueFinanceReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build responses used for mocking
                _shipToDestinationsCollection = new List<ShipToDestination>()
                {
                    new ShipToDestination("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new ShipToDestination("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new ShipToDestination("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

                // Build responses used for mocking
                _shipToCodesCollection = new List<ShipToCode>()
                {
                    new ShipToCode("MC", "Main Campus"),
                    new ShipToCode("EC", "East Campus"),
                    new ShipToCode("SC", "South Campus")
                };
                // Build repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                codeItemName = referenceDataRepo.BuildFullCacheKey("AllShipToDestinations");
            }

            [TestMethod]
            public async Task GetsShipToCodesAsync()
            {
                // Setup response to ShipToCodes read
                var entityCollection = new Collection<ShipToCodes>(_shipToCodesCollection.Select(record =>
                    new Data.ColleagueFinance.DataContracts.ShipToCodes()
                    {
                        Recordkey = record.Code,
                        ShptName = record.Description,
                    }).ToList());

                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<ShipToCodes>("SHIP.TO.CODES", "", true))
                    .ReturnsAsync(entityCollection);

                var result = await referenceDataRepo.GetShipToCodesAsync();

                for (int i = 0; i < _shipToCodesCollection.Count(); i++)
                {
                    Assert.AreEqual(_shipToCodesCollection.ElementAt(i).Code, result.ElementAt(i).Code);
                    Assert.AreEqual(_shipToCodesCollection.ElementAt(i).Description, result.ElementAt(i).Description);
                }
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                _shipToDestinationsCollection = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task GetsShipToDestinationCacheAsync()
            {
                var result = await referenceDataRepo.GetShipToDestinationsAsync(false);

                for (int i = 0; i < _shipToDestinationsCollection.Count(); i++)
                {
                    Assert.AreEqual(_shipToDestinationsCollection.ElementAt(i).Guid, result.ElementAt(i).Guid);
                    Assert.AreEqual(_shipToDestinationsCollection.ElementAt(i).Code, result.ElementAt(i).Code);
                    Assert.AreEqual(_shipToDestinationsCollection.ElementAt(i).Description, result.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetsShipToDestinationNonCacheAsync()
            {
                var result = await referenceDataRepo.GetShipToDestinationsAsync(true);

                for (int i = 0; i < _shipToDestinationsCollection.Count(); i++)
                {
                    Assert.AreEqual(_shipToDestinationsCollection.ElementAt(i).Guid, result.ElementAt(i).Guid);
                    Assert.AreEqual(_shipToDestinationsCollection.ElementAt(i).Code, result.ElementAt(i).Code);
                    Assert.AreEqual(_shipToDestinationsCollection.ElementAt(i).Description, result.ElementAt(i).Description);
                }
            }

            private ColleagueFinanceReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to ShipToCodes read
                var entityCollection = new Collection<ShipToCodes>(_shipToDestinationsCollection.Select(record =>
                    new Data.ColleagueFinance.DataContracts.ShipToCodes()
                    {
                        Recordkey = record.Code,
                        ShptName = record.Description,
                        RecordGuid = record.Guid
                    }).ToList());

                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<ShipToCodes>("SHIP.TO.CODES", "", true))
                    .ReturnsAsync(entityCollection);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));


                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var entity = _shipToDestinationsCollection.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "SHIP.TO.CODES", entity.Code }),
                            new RecordKeyLookupResult() { Guid = entity.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new ColleagueFinanceReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

                return referenceDataRepo;
            }
        }

        /// <summary>
        /// Test class for AccountsPayableSources codes
        /// </summary>
        [TestClass]
        public class AccountsPayableSourcesTests
        {
            private Mock<IColleagueTransactionFactory> transFactoryMock;
            private Mock<ICacheProvider> cacheProviderMock;
            private Mock<IColleagueDataReader> dataAccessorMock;
            private Mock<ILogger> loggerMock;
            private IEnumerable<AccountsPayableSources> allAccountsPayableSources;
            private string codeItemName;

            private ColleagueFinanceReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build responses used for mocking
                allAccountsPayableSources = new TestColleagueFinanceReferenceDataRepository().GetAccountsPayableSourcesAsync(false).Result;
                foreach (var acct in allAccountsPayableSources)
                    acct.directDeposit = "Y";

                // Build repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                codeItemName = referenceDataRepo.BuildFullCacheKey("AllAccountsPayableSources");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                allAccountsPayableSources = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task GetsAccountsPayableSourcesCacheAsync()
            {
                var accountsPayableSources = await referenceDataRepo.GetAccountsPayableSourcesAsync(false);

                for (int i = 0; i < allAccountsPayableSources.Count(); i++)
                {
                    Assert.AreEqual(allAccountsPayableSources.ElementAt(i).Guid, accountsPayableSources.ElementAt(i).Guid);
                    Assert.AreEqual(allAccountsPayableSources.ElementAt(i).Code, accountsPayableSources.ElementAt(i).Code);
                    Assert.AreEqual(allAccountsPayableSources.ElementAt(i).Description, accountsPayableSources.ElementAt(i).Description);
                    Assert.AreEqual(allAccountsPayableSources.ElementAt(i).directDeposit, "Y");
                }
            }

            [TestMethod]
            public async Task GetsAccountsPayableSourcesNonCacheAsync()
            {
                var accountsPayableSources = await referenceDataRepo.GetAccountsPayableSourcesAsync(true);

                for (int i = 0; i < allAccountsPayableSources.Count(); i++)
                {
                    Assert.AreEqual(allAccountsPayableSources.ElementAt(i).Guid, accountsPayableSources.ElementAt(i).Guid);
                    Assert.AreEqual(allAccountsPayableSources.ElementAt(i).Code, accountsPayableSources.ElementAt(i).Code);
                    Assert.AreEqual(allAccountsPayableSources.ElementAt(i).Description, accountsPayableSources.ElementAt(i).Description);
                    Assert.AreEqual(allAccountsPayableSources.ElementAt(i).directDeposit, "Y");
                }
            }

            private ColleagueFinanceReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to AccountsPayableSources read
                var apTypesCollection = new Collection<ApTypes>(allAccountsPayableSources.Select(record =>
                    new Data.ColleagueFinance.DataContracts.ApTypes()
                    {
                        Recordkey = record.Code,
                        ApTypesDesc = record.Description,
                        RecordGuid = record.Guid,
                        AptBankCode = "B01"
                    }).ToList());

                var bankCodesCollection = new Collection<Base.DataContracts.BankCodes>();
                var bankCode = new Base.DataContracts.BankCodes();
                bankCode.Recordkey = "B01";
                bankCode.BankEftActiveFlag = "Y";
                bankCodesCollection.Add(bankCode);


                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<ApTypes>("AP.TYPES", "", true))
                    .ReturnsAsync(apTypesCollection);
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<Base.DataContracts.BankCodes>("BANK.CODES", "", true))
                    .ReturnsAsync(bankCodesCollection);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));




                // Construct repository
                referenceDataRepo = new ColleagueFinanceReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

                return referenceDataRepo;
            }
        }

        /// <summary>
        /// Test class for VendorHoldReasons
        /// </summary>
        [TestClass]
        public class VendorHoldReasonsTests
        {
            private Mock<IColleagueTransactionFactory> transFactoryMock;
            private Mock<ICacheProvider> cacheProviderMock;
            private Mock<IColleagueDataReader> dataAccessorMock;
            private Mock<ILogger> loggerMock;
            private IEnumerable<VendorHoldReasons> allVendorHoldReasons;
            private ApplValcodes intgVendorHoldReasons;
            private string valcodeName;

            private ColleagueFinanceReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build responses used for mocking
                allVendorHoldReasons = new TestColleagueFinanceReferenceDataRepository().GetVendorHoldReasonsAsync(false).Result;
                intgVendorHoldReasons = BuildValcodeResponse(allVendorHoldReasons);

                // Build repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("CF_VENDOR_HOLD_REASONS_GUID");

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                    x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                allVendorHoldReasons = null;
                referenceDataRepo = null;
                intgVendorHoldReasons = null;
            }

            [TestMethod]
            public async Task GetsVendorHoldReasonsCacheAsync()
            {
                var vendorHoldReasons = await referenceDataRepo.GetVendorHoldReasonsAsync(false);

                for (int i = 0; i < allVendorHoldReasons.Count(); i++)
                {
                    Assert.AreEqual(allVendorHoldReasons.ElementAt(i).Guid, vendorHoldReasons.ElementAt(i).Guid);
                    Assert.AreEqual(allVendorHoldReasons.ElementAt(i).Code, vendorHoldReasons.ElementAt(i).Code);
                    Assert.AreEqual(allVendorHoldReasons.ElementAt(i).Description, vendorHoldReasons.ElementAt(i).Description);

                }
            }

            [TestMethod]
            public async Task GetsVendorHoldReasonsNonCacheAsync()
            {
                var vendorHoldReasons = await referenceDataRepo.GetVendorHoldReasonsAsync(true);

                for (int i = 0; i < allVendorHoldReasons.Count(); i++)
                {
                    Assert.AreEqual(allVendorHoldReasons.ElementAt(i).Guid, vendorHoldReasons.ElementAt(i).Guid);
                    Assert.AreEqual(allVendorHoldReasons.ElementAt(i).Code, vendorHoldReasons.ElementAt(i).Code);
                    Assert.AreEqual(allVendorHoldReasons.ElementAt(i).Description, vendorHoldReasons.ElementAt(i).Description);

                }
            }

            //[TestMethod]
            //public async Task GetsVendorHoldReasonsWritesToCacheAsync()
            //{

            //    // Set up local cache mock to respond to cache request:
            //    //  -to "Contains" request, return "false" to indicate item is not in cache
            //    //  -to cache "Get" request, return null so we know it's reading from the "repository"
            //    cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(false);
            //    cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(null);

            //    // return a valid response to the data accessor request
            //    dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CF.VALCODES", "INTG.VENDOR.HOLD.REASONS", It.IsAny<bool>())).ReturnsAsync(intgVendorHoldReasons);

            //    cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
            //     x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
            //     .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

            //    // But after data accessor read, set up mocking so we can verify the list of vendor hold reasons was written to the cache
            //    cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<List<VendorHoldReasons>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

            //    cacheProviderMock.Setup(x => x.Contains(referenceDataRepo.BuildFullCacheKey("INTG.VENDOR.HOLD.REASONS"), null)).Returns(true);
            //    var vendorHoldReasons = await referenceDataRepo.GetVendorHoldReasonsAsync(false);
            //    cacheProviderMock.Setup(x => x.Get(referenceDataRepo.BuildFullCacheKey("INTG.VENDOR.HOLD.REASONS"), null)).Returns(vendorHoldReasons);
            //    // Verify that vendorHoldReasons were returned, which means they came from the "repository".
            //    Assert.IsTrue(vendorHoldReasons.Count() == 3);

            //    // Verify that the vendorHoldReasons item was added to the cache after it was read from the repository
            //    cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<List<VendorHoldReasons>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);

            //}

            //[TestMethod]
            //public async Task GetsVendorHoldReasonsGetsCachedAsync()
            //{
            //    // Set up local cache mock to respond to cache request:
            //    //  -to "Contains" request, return "true" to indicate item is in cache
            //    //  -to "Get" request, return the cache item (in this case the "INTG.VENDOR.HOLD.REASONS" cache item)
            //    cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(true);
            //    cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(allVendorHoldReasons).Verifiable();

            //    // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
            //    dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CF.VALCODES", "INTG.VENDOR.HOLD.REASONS", true)).ReturnsAsync(new ApplValcodes());

            //    // Assert the vendorHoldReasons are returned
            //    var actual = await referenceDataRepo.GetVendorHoldReasonsAsync(false);
            //    Assert.IsTrue(actual.Count() == 4);
            //    // Verify that the vendorHoldReasons were retrieved from cache
            //    cacheProviderMock.Verify(m => m.Get(valcodeName, null));
            //}


            private ApplValcodes BuildValcodeResponse(IEnumerable<VendorHoldReasons> vendorHoldreasons)
            {
                ApplValcodes vendorHoldReasonsResponse = new ApplValcodes();
                vendorHoldReasonsResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in vendorHoldreasons)
                {
                    vendorHoldReasonsResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "2", item.Code, "3", "", ""));
                }
                return vendorHoldReasonsResponse;
            }

            private ColleagueFinanceReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to vendorHoldReasons valcode read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CF.VALCODES", "INTG.VENDOR.HOLD.REASONS", It.IsAny<bool>())).ReturnsAsync(intgVendorHoldReasons);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var vendorHoldReason = allVendorHoldReasons.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] {"CF.VALCODES", "INTG.VENDOR.HOLD.REASONS", vendorHoldReason.Code}),
                            new RecordKeyLookupResult() {Guid = vendorHoldReason.Guid});
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new ColleagueFinanceReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

                return referenceDataRepo;
            }
        }

        /// <summary>
        ///  Test class for Vendor Types
        /// </summary>
        [TestClass]
        public class VendorTypesTests : BaseRepositorySetup
        {
            private IEnumerable<Ellucian.Colleague.Domain.ColleagueFinance.Entities.VendorType> testDataRepository;

            private ColleagueFinanceReferenceDataRepository actualRepository;

            private Mock<IColleagueDataReader> dataAccessorMock;

            private Mock<IColleagueTransactionFactory> transFactoryMock;

            [TestInitialize]
            public void ColleagueFinanceReferenceDataRepositoryTests()
            {


                MockInitialize();
                dataAccessorMock = new Mock<IColleagueDataReader>();

                testDataRepository = new TestColleagueFinanceReferenceDataRepository().GetVendorTypesAsync(false).Result;

                transFactoryMock = new Mock<IColleagueTransactionFactory>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                actualRepository = BuildRepository();
            }

            public ColleagueFinanceReferenceDataRepository BuildRepository()
            {
                var records = new Collection<DataContracts.VendorTypes>();

                foreach (var item in testDataRepository)
                {
                    DataContracts.VendorTypes record = new DataContracts.VendorTypes();
                    record.RecordGuid = item.Guid;
                    record.VendorTypesDesc = item.Description;
                    record.Recordkey = item.Code;
                    records.Add(record);
                }

                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.VendorTypes>("VENDOR.TYPES", "", true)).ReturnsAsync(records);

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var record = testDataRepository.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] {"VENDOR.TYPES", record.Code}),
                            new RecordKeyLookupResult() {Guid = record.Guid});
                    }
                    return Task.FromResult(result);
                });

                apiSettings.BulkReadSize = 1;

                loggerMock.Setup(l => l.IsErrorEnabled).Returns(true);

                return new ColleagueFinanceReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            }

            public async Task<IEnumerable<VendorType>> getExpectedVendorTypes()
            {
                return testDataRepository;
            }

            public async Task<IEnumerable<VendorType>> getActualVendorTypes(bool ignoreCache = false)
            {
                return await actualRepository.GetVendorTypesAsync(ignoreCache);
            }

            [TestMethod]
            public async Task ExpectedEqualsActualTest()
            {
                var expected = (await getExpectedVendorTypes()).ToList();
                var actual = (await getActualVendorTypes()).ToList();
                CollectionAssert.AreEqual(expected, actual);
            }

            [TestMethod]
            public async Task AttributesTest()
            {
                var expected = (await getExpectedVendorTypes()).ToArray();
                var actual = (await getActualVendorTypes()).ToArray();
                for (int i = 0; i < expected.Count(); i++)
                {
                    Assert.AreEqual(expected[i].Code, actual[i].Code);
                    Assert.AreEqual(expected[i].Guid, actual[i].Guid);
                    Assert.AreEqual(expected[i].Description, actual[i].Description);
                }
            }

            [TestMethod]
            public async Task ExpectedEqualsActualTest_Cached()
            {
                var expected = (await getExpectedVendorTypes()).ToList();
                var actual = (await getActualVendorTypes(true)).ToList();
                CollectionAssert.AreEqual(expected, actual);
            }

        }

        /// <summary>
        /// Test class for VendorTerm codes
        /// </summary>
        [TestClass]
        public class VendorTermTests
        {
            private Mock<IColleagueTransactionFactory> transFactoryMock;
            private Mock<ICacheProvider> cacheProviderMock;
            private Mock<IColleagueDataReader> dataAccessorMock;
            private Mock<ILogger> loggerMock;
            private IEnumerable<VendorTerm> _vendorTermCollection;
            private string codeItemName;

            private ColleagueFinanceReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build responses used for mocking
                _vendorTermCollection = new List<VendorTerm>()
                {
                    new VendorTerm("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new VendorTerm("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new VendorTerm("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

                // Build repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                codeItemName = referenceDataRepo.BuildFullCacheKey("AllVendorTerm");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                _vendorTermCollection = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task GetsVendorTermCacheAsync()
            {
                var result = await referenceDataRepo.GetVendorTermsAsync(false);

                for (int i = 0; i < _vendorTermCollection.Count(); i++)
                {
                    Assert.AreEqual(_vendorTermCollection.ElementAt(i).Guid, result.ElementAt(i).Guid);
                    Assert.AreEqual(_vendorTermCollection.ElementAt(i).Code, result.ElementAt(i).Code);
                    Assert.AreEqual(_vendorTermCollection.ElementAt(i).Description, result.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetsVendorTermNonCacheAsync()
            {
                var result = await referenceDataRepo.GetVendorTermsAsync(true);

                for (int i = 0; i < _vendorTermCollection.Count(); i++)
                {
                    Assert.AreEqual(_vendorTermCollection.ElementAt(i).Guid, result.ElementAt(i).Guid);
                    Assert.AreEqual(_vendorTermCollection.ElementAt(i).Code, result.ElementAt(i).Code);
                    Assert.AreEqual(_vendorTermCollection.ElementAt(i).Description, result.ElementAt(i).Description);
                }
            }

            private ColleagueFinanceReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to VendorTerm read
                var entityCollection = new Collection<VendorTerms>(_vendorTermCollection.Select(record =>
                    new Data.ColleagueFinance.DataContracts.VendorTerms()
                    {
                        Recordkey = record.Code,
                        VendorTermsDesc = record.Description,
                        RecordGuid = record.Guid
                    }).ToList());

                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<VendorTerms>("VENDOR.TERMS", "", true))
                    .ReturnsAsync(entityCollection);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));


                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var entity = _vendorTermCollection.FirstOrDefault(e => e.Code == recordKeyLookup.PrimaryKey);
                        result.Add(string.Join("+", new string[] {"VENDOR.TERMS", entity.Code}),
                            new RecordKeyLookupResult() {Guid = entity.Guid});
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new ColleagueFinanceReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

                return referenceDataRepo;
            }
        }

        /// <summary>
        /// Test class for Fiscal Years codes
        /// </summary>
        [TestClass]
        public class FiscalYearsTests
        {
            private Mock<IColleagueTransactionFactory> transFactoryMock;
            private Mock<ICacheProvider> cacheProviderMock;
            private Mock<IColleagueDataReader> dataAccessorMock;
            private Mock<ILogger> loggerMock;
            private IEnumerable<FiscalYear> allFiscalYears;
            private string codeItemName;

            private ColleagueFinanceReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build responses used for mocking
                allFiscalYears = new TestColleagueFinanceReferenceDataRepository().GetFiscalYearsAsync(false).Result;
                       
                // Build repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                codeItemName = referenceDataRepo.BuildFullCacheKey("AllFiscalYearsGenLdgr");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                allFiscalYears = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task GetsFiscalYearsCacheAsync()
            {
                var fiscalYears = await referenceDataRepo.GetFiscalYearsAsync(false);

                for (int i = 0; i < allFiscalYears.Count(); i++)
                {
                    Assert.AreEqual(allFiscalYears.ElementAt(i).Guid, fiscalYears.ElementAt(i).Guid);
                    Assert.AreEqual(allFiscalYears.ElementAt(i).Title, fiscalYears.ElementAt(i).Title);
                   
                }
            }

            [TestMethod]
            public async Task GetsFiscalYearsNonCacheAsync()
            {
                var fiscalYears = await referenceDataRepo.GetFiscalYearsAsync(true);

                for (int i = 0; i < allFiscalYears.Count(); i++)
                {
                    Assert.AreEqual(allFiscalYears.ElementAt(i).Guid, fiscalYears.ElementAt(i).Guid);
                    Assert.AreEqual(allFiscalYears.ElementAt(i).Title, fiscalYears.ElementAt(i).Title);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task GetsFiscalYearsNonCacheAsync_EmptyCorpName()
            {
                // Setup response to FiscalYear read
                var genLedrCollection = new Collection<GenLdgr>(allFiscalYears.Select(record =>
                    new Data.ColleagueFinance.DataContracts.GenLdgr()
                    {
                        RecordGuid = record.Guid,
                        Recordkey = record.Id,
                        GenLdgrDescription = record.Title,
                        GenLdgrStatus = record.Status

                    }).ToList());

                var _fiscalYrDataContract = new Fiscalyr()
                {
                    Recordkey = "1",
                    CfCurrentFiscalYear = "2017",
                    FiscalStartMonth = 7
                };

                var corp = new Corp() { Recordkey = "1", CorpParents = new List<string>() { "3" }, CorpName = null };

                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<DataContracts.Fiscalyr>("ACCOUNT.PARAMETERS", "FISCAL.YEAR", It.IsAny<bool>()))
                    .ReturnsAsync(_fiscalYrDataContract);

                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<GenLdgr>("GEN.LDGR", It.IsAny<string>(), true))
                    .ReturnsAsync(genLedrCollection);

                dataAccessorMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>("PERSON", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(corp);

                var fiscalYears = await referenceDataRepo.GetFiscalYearsAsync(true);
            }

            [TestMethod]
            [ExpectedException(typeof(ConfigurationException))]
            public async Task GetsFiscalYearsNonCacheAsync_FiscalStartMonth_Invalid()
            {
                // Setup response to FiscalYear read
                var genLedrCollection = new Collection<GenLdgr>(allFiscalYears.Select(record =>
                    new Data.ColleagueFinance.DataContracts.GenLdgr()
                    {
                        RecordGuid = record.Guid,
                        Recordkey = record.Id,
                        GenLdgrDescription = record.Title,
                        GenLdgrStatus = record.Status

                    }).ToList());

                var _fiscalYrDataContract = new Fiscalyr()
                {
                    Recordkey = "1",
                    CfCurrentFiscalYear = "2017",
                    FiscalStartMonth = 13
                };

                var corp = new Corp() { Recordkey = "1", CorpParents = new List<string>() { "3" }, CorpName = new List<string>() { "Ellucian University" } };

                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<DataContracts.Fiscalyr>("ACCOUNT.PARAMETERS", "FISCAL.YEAR", It.IsAny<bool>()))
                    .ReturnsAsync(_fiscalYrDataContract);

                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<GenLdgr>("GEN.LDGR", It.IsAny<string>(), true))
                    .ReturnsAsync(genLedrCollection);

                dataAccessorMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>("PERSON", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(corp);

                var fiscalYears = await referenceDataRepo.GetFiscalYearsAsync(true);
            }

            [TestMethod]
            [ExpectedException(typeof(ConfigurationException))]
            public async Task GetsFiscalYearsNonCacheAsync_FiscalStartMonth_Empty()
            {
                // Setup response to FiscalYear read
                var genLedrCollection = new Collection<GenLdgr>(allFiscalYears.Select(record =>
                    new Data.ColleagueFinance.DataContracts.GenLdgr()
                    {
                        RecordGuid = record.Guid,
                        Recordkey = record.Id,
                        GenLdgrDescription = record.Title,
                        GenLdgrStatus = record.Status

                    }).ToList());

                var _fiscalYrDataContract = new Fiscalyr()
                {
                    Recordkey = "1",
                    CfCurrentFiscalYear = "2017",
                   
                };

                var corp = new Corp() { Recordkey = "1", CorpParents = new List<string>() { "3" }, CorpName = new List<string>() { "Ellucian University" } };

                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<DataContracts.Fiscalyr>("ACCOUNT.PARAMETERS", "FISCAL.YEAR", It.IsAny<bool>()))
                    .ReturnsAsync(_fiscalYrDataContract);

                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<GenLdgr>("GEN.LDGR", It.IsAny<string>(), true))
                    .ReturnsAsync(genLedrCollection);

                dataAccessorMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>("PERSON", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(corp);

                var fiscalYears = await referenceDataRepo.GetFiscalYearsAsync(true);
            }


            [TestMethod]
            [ExpectedException(typeof(ConfigurationException))]
            public async Task GetsFiscalYearsNonCacheAsync_GenLdgr_Empty()
            {
                
                var _fiscalYrDataContract = new Fiscalyr()
                {
                    Recordkey = "1",
                    CfCurrentFiscalYear = "2017",
                    FiscalStartMonth = 13
                };

                var corp = new Corp() { Recordkey = "1", CorpParents = new List<string>() { "3" }, CorpName = new List<string>() { "Ellucian University" }  };

                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<DataContracts.Fiscalyr>("ACCOUNT.PARAMETERS", "FISCAL.YEAR", It.IsAny<bool>()))
                    .ReturnsAsync(_fiscalYrDataContract);

                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<GenLdgr>("GEN.LDGR", It.IsAny<string>(), true))
                    .ReturnsAsync(null);

                dataAccessorMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>("PERSON", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(corp);

                var fiscalYears = await referenceDataRepo.GetFiscalYearsAsync(true);
            }

            private ColleagueFinanceReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to FiscalYear read
                var genLedrCollection = new Collection<GenLdgr>(allFiscalYears.Select(record =>
                    new Data.ColleagueFinance.DataContracts.GenLdgr()
                    {                                    
                        RecordGuid = record.Guid,
                        Recordkey = record.Id,
                        GenLdgrDescription = record.Title,
                        GenLdgrStatus = record.Status
                        
                    }).ToList());

                var _fiscalYrDataContract = new Fiscalyr()
                {
                    Recordkey = "1",
                    CfCurrentFiscalYear = "2017",
                    FiscalStartMonth = 7
                };

                var corp = new Corp() { Recordkey = "1", CorpParents = new List<string>() { "3" }, CorpName = new List<string>() { "Ellucian University" } };

                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<DataContracts.Fiscalyr>("ACCOUNT.PARAMETERS", "FISCAL.YEAR", It.IsAny<bool>()))
                    .ReturnsAsync(_fiscalYrDataContract);

                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<GenLdgr>("GEN.LDGR", It.IsAny<string>(), true))
                    .ReturnsAsync(genLedrCollection);

                dataAccessorMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Corp>("PERSON", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(corp);


                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // Construct repository
                referenceDataRepo = new ColleagueFinanceReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

                return referenceDataRepo;
            }
        }

        /// <summary>
        /// Test class for Fiscal Periods codes
        /// </summary>
        [TestClass]
        public class FiscalPeriodsTests
        {
            private Mock<IColleagueTransactionFactory> transFactoryMock;
            private Mock<ICacheProvider> cacheProviderMock;
            private Mock<IColleagueDataReader> dataAccessorMock;
            private Mock<ILogger> loggerMock;
            private IEnumerable<Domain.ColleagueFinance.Entities.FiscalPeriodsIntg> allFiscalPeriodsIntg;
            private string codeItemName;

            private ColleagueFinanceReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build responses used for mocking
                allFiscalPeriodsIntg = new TestColleagueFinanceReferenceDataRepository().GetFiscalPeriodsIntgAsync(false).Result;

                // Build repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                codeItemName = referenceDataRepo.BuildFullCacheKey("AllFiscalPeriodIntgs");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                allFiscalPeriodsIntg = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task GetsFiscalPeriodsIntgCacheAsync()
            {
                var fiscalPeriodsIntg = await referenceDataRepo.GetFiscalPeriodsIntgAsync(false);

                for (int i = 0; i < allFiscalPeriodsIntg.Count(); i++)
                {
                    Assert.AreEqual(allFiscalPeriodsIntg.ElementAt(i).Guid, fiscalPeriodsIntg.ElementAt(i).Guid);
                    Assert.AreEqual(allFiscalPeriodsIntg.ElementAt(i).Id, fiscalPeriodsIntg.ElementAt(i).Id);
                    Assert.AreEqual(allFiscalPeriodsIntg.ElementAt(i).Status, fiscalPeriodsIntg.ElementAt(i).Status);
                }
            }

            [TestMethod]
            public async Task GetsFiscalPeriodsIntgNonCacheAsync()
            {
                var fiscalPeriodsIntg = await referenceDataRepo.GetFiscalPeriodsIntgAsync(true);

                for (int i = 0; i < allFiscalPeriodsIntg.Count(); i++)
                {
                    Assert.AreEqual(allFiscalPeriodsIntg.ElementAt(i).Guid, fiscalPeriodsIntg.ElementAt(i).Guid);
                    Assert.AreEqual(allFiscalPeriodsIntg.ElementAt(i).Id, fiscalPeriodsIntg.ElementAt(i).Id);
                    Assert.AreEqual(allFiscalPeriodsIntg.ElementAt(i).Status, fiscalPeriodsIntg.ElementAt(i).Status);

                }
            }


            private ColleagueFinanceReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to FiscalPeriodIntg read
                var fiscalPeriodIntgCollection = new Collection<Data.ColleagueFinance.DataContracts.FiscalPeriodsIntg>(allFiscalPeriodsIntg.Select(record =>
                    new Data.ColleagueFinance.DataContracts.FiscalPeriodsIntg()
                    {
                        RecordGuid = record.Guid,
                        Recordkey = record.Id,
                        FpiFiscalYear = Convert.ToInt16(record.Id),
                        FpiCurrentStatus = record.Status

                    }).ToList());
                           
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<Data.ColleagueFinance.DataContracts.FiscalPeriodsIntg>("FISCAL.PERIODS.INTG", It.IsAny<string>(), true))
                    .ReturnsAsync(fiscalPeriodIntgCollection);

                var _fiscalYrDataContract = new Fiscalyr()
                {
                    Recordkey = "1",
                    CfCurrentFiscalYear = "2016",
                    FiscalStartMonth = 7
                };

                var corp = new Corp() { Recordkey = "1", CorpParents = new List<string>() { "3" }, CorpName = new List<string>() { "Ellucian University" } };

                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<DataContracts.Fiscalyr>("ACCOUNT.PARAMETERS", "FISCAL.YEAR", It.IsAny<bool>()))
                    .ReturnsAsync(_fiscalYrDataContract);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // Construct repository
                referenceDataRepo = new ColleagueFinanceReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

                return referenceDataRepo;
            }
        }

        [TestClass]
        public class AccountingStringSubcomponentValuesTests
        {
            private Mock<IColleagueTransactionFactory> transFactoryMock;
            private Mock<ICacheProvider> cacheProviderMock;
            private Mock<IColleagueDataReader> dataAccessorMock;
            private Mock<ILogger> loggerMock;
            private List<AccountingStringSubcomponentValues> allAstrsValues;
            private string codeItemName;

            private ColleagueFinanceReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build responses used for mocking
                allAstrsValues = new TestColleagueFinanceReferenceDataRepository().GetAccountingStringSubcomponentValuesAsync(false).Result.ToList();

                // Build repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                codeItemName = referenceDataRepo.BuildFullCacheKey("AllAccountingStringSubcomponentValues");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                allAstrsValues = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task GetsAccountingStringSubcomponentValuesAsync()
            {
                var assv = await referenceDataRepo.GetAccountingStringSubcomponentValuesAsync(0, 3, It.IsAny<bool>());

                for (int i = 0; i < allAstrsValues.Count(); i++)
                {
                    Assert.AreEqual(allAstrsValues.ElementAt(i).Guid, assv.Item1.ElementAt(i).Guid);
                    Assert.AreEqual(allAstrsValues.ElementAt(i).Code, assv.Item1.ElementAt(i).Code);
                    Assert.AreEqual(allAstrsValues.ElementAt(i).Description, assv.Item1.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetsAccountingStringSubcomponentValueByGuidAsync_FD_DESCS()
            {
                string guid = "884a59d1-20e5-43af-94e3-f1504230bbbc";
                GuidLookup[] lookup = new GuidLookup[] { new GuidLookup(guid) };
                var lookUpResults = new Dictionary<string, GuidLookupResult>();
                lookUpResults.Add("FD.DESCS", new GuidLookupResult() { Entity = "FD.DESCS", PrimaryKey = "1", SecondaryKey = "Somekey" });
                dataAccessorMock.Setup(i => i.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResults);
                dataAccessorMock.Setup(repo => repo.ReadRecordAsync<FdDescs>("1", It.IsAny<bool>()))
                    .ReturnsAsync(new FdDescs() { FdDescription = "Descr", RecordGuid = guid, Recordkey = "1", FdExplanation = "FdExplanation" });
                var assv = await referenceDataRepo.GetAccountingStringSubcomponentValuesByGuidAsync(guid);
                Assert.IsNotNull(assv);
            }

            [TestMethod]
            public async Task GetsAccountingStringSubcomponentValueByGuidAsync_UN_DESCS()
            {
                string guid = "884a59d1-20e5-43af-94e3-f1504230bbbc";
                GuidLookup[] lookup = new GuidLookup[] { new GuidLookup(guid) };
                var lookUpResults = new Dictionary<string, GuidLookupResult>();
                lookUpResults.Add("UN.DESCS", new GuidLookupResult() { Entity = "UN.DESCS", PrimaryKey = "1", SecondaryKey = "Somekey" });
                dataAccessorMock.Setup(i => i.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResults);
                dataAccessorMock.Setup(repo => repo.ReadRecordAsync<UnDescs>("1", It.IsAny<bool>()))
                    .ReturnsAsync(new UnDescs() { UnDescription = "Descr", RecordGuid = guid, Recordkey = "1", UnExplanation = "FdExplanation" });
                var assv = await referenceDataRepo.GetAccountingStringSubcomponentValuesByGuidAsync(guid);
                Assert.IsNotNull(assv);
            }

            [TestMethod]
            public async Task GetsAccountingStringSubcomponentValueByGuidAsync_OB_DESCS()
            {
                string guid = "884a59d1-20e5-43af-94e3-f1504230bbbc";
                GuidLookup[] lookup = new GuidLookup[] { new GuidLookup(guid) };
                var lookUpResults = new Dictionary<string, GuidLookupResult>();
                lookUpResults.Add("OB.DESCS", new GuidLookupResult() { Entity = "OB.DESCS", PrimaryKey = "1", SecondaryKey = "Somekey" });
                dataAccessorMock.Setup(i => i.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResults);
                dataAccessorMock.Setup(repo => repo.ReadRecordAsync<ObDescs>("1", It.IsAny<bool>()))
                    .ReturnsAsync(new ObDescs() { ObDescription = "Descr", RecordGuid = guid, Recordkey = "1", ObExplanation = "FdExplanation" });
                var assv = await referenceDataRepo.GetAccountingStringSubcomponentValuesByGuidAsync(guid);
                Assert.IsNotNull(assv);
            }

            [TestMethod]
            public async Task GetsAccountingStringSubcomponentValueByGuidAsync_FC_DESCS()
            {
                string guid = "884a59d1-20e5-43af-94e3-f1504230bbbc";
                GuidLookup[] lookup = new GuidLookup[] { new GuidLookup(guid) };
                var lookUpResults = new Dictionary<string, GuidLookupResult>();
                lookUpResults.Add("FC.DESCS", new GuidLookupResult() { Entity = "FC.DESCS", PrimaryKey = "1", SecondaryKey = "Somekey" });
                dataAccessorMock.Setup(i => i.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResults);
                dataAccessorMock.Setup(repo => repo.ReadRecordAsync<FcDescs>("1", It.IsAny<bool>()))
                    .ReturnsAsync(new FcDescs() { FcDescription = "Descr", RecordGuid = guid, Recordkey = "1", FcExplanation = "FdExplanation" });
                var assv = await referenceDataRepo.GetAccountingStringSubcomponentValuesByGuidAsync(guid);
                Assert.IsNotNull(assv);
            }

            [TestMethod]
            public async Task GetsAccountingStringSubcomponentValueByGuidAsync_SO_DESCS()
            {
                string guid = "884a59d1-20e5-43af-94e3-f1504230bbbc";
                GuidLookup[] lookup = new GuidLookup[] { new GuidLookup(guid) };
                var lookUpResults = new Dictionary<string, GuidLookupResult>();
                lookUpResults.Add("SO.DESCS", new GuidLookupResult() { Entity = "SO.DESCS", PrimaryKey = "1", SecondaryKey = "Somekey" });
                dataAccessorMock.Setup(i => i.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResults);
                dataAccessorMock.Setup(repo => repo.ReadRecordAsync<SoDescs>("1", It.IsAny<bool>()))
                    .ReturnsAsync(new SoDescs() { SoDescription = "Descr", RecordGuid = guid, Recordkey = "1", SoExplanation = "FdExplanation" });
                var assv = await referenceDataRepo.GetAccountingStringSubcomponentValuesByGuidAsync(guid);
                Assert.IsNotNull(assv);
            }

            [TestMethod]
            public async Task GetsAccountingStringSubcomponentValueByGuidAsync_LO_DESCS()
            {
                string guid = "884a59d1-20e5-43af-94e3-f1504230bbbc";
                GuidLookup[] lookup = new GuidLookup[] { new GuidLookup(guid) };
                var lookUpResults = new Dictionary<string, GuidLookupResult>();
                lookUpResults.Add("LO.DESCS", new GuidLookupResult() { Entity = "LO.DESCS", PrimaryKey = "1", SecondaryKey = "Somekey" });
                dataAccessorMock.Setup(i => i.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResults);
                dataAccessorMock.Setup(repo => repo.ReadRecordAsync<LoDescs>("1", It.IsAny<bool>()))
                    .ReturnsAsync(new LoDescs() { LoDescription = "Descr", RecordGuid = guid, Recordkey = "1", LoExplanation = "FdExplanation" });
                var assv = await referenceDataRepo.GetAccountingStringSubcomponentValuesByGuidAsync(guid);
                Assert.IsNotNull(assv);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetsAccountingStringSubcomponentValueByGuidAsync_Default_KeyNotFoundException()
            {
                string guid = "884a59d1-20e5-43af-94e3-f1504230bbbc";
                GuidLookup[] lookup = new GuidLookup[] { new GuidLookup(guid) };
                var lookUpResults = new Dictionary<string, GuidLookupResult>();
                lookUpResults.Add("PERSONS", new GuidLookupResult() { Entity = "PERSONS", PrimaryKey = "1", SecondaryKey = "Somekey" });
                dataAccessorMock.Setup(i => i.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResults);
                dataAccessorMock.Setup(repo => repo.ReadRecordAsync<LoDescs>("1", It.IsAny<bool>()))
                    .ReturnsAsync(new LoDescs() { LoDescription = "Descr", RecordGuid = guid, Recordkey = "1", LoExplanation = "FdExplanation" });
                var assv = await referenceDataRepo.GetAccountingStringSubcomponentValuesByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetsAccountingStringSubcomponentValueByGuidAsync_NoRecordInfo_KeyNotFoundException()
            {
                string guid = "884a59d1-20e5-43af-94e3-f1504230bbbc";
                GuidLookup[] lookup = new GuidLookup[] { new GuidLookup(guid) };
                Dictionary<string, GuidLookupResult> lookUpResults = new Dictionary<string, GuidLookupResult>();
                lookUpResults.Add("LO.DESCS", null);
                dataAccessorMock.Setup(i => i.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResults);
               
                var assv = await referenceDataRepo.GetAccountingStringSubcomponentValuesByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetsAccountingStringSubcomponentValueByGuidAsync_ArgumentNullException()
            {
                var assv = await referenceDataRepo.GetAccountingStringSubcomponentValuesByGuidAsync("");
            }
            private ColleagueFinanceReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to CommodityUnitTypes read
                var fdCollection = new Collection<FdDescs>();
                var unCollection = new Collection<UnDescs>();
                var obCollection = new Collection<ObDescs>();
                var fcCollection = new Collection<FcDescs>();
                var soCollection = new Collection<SoDescs>();
                var loCollection = new Collection<LoDescs>();

                allAstrsValues.ToList().ForEach(record =>
                    fdCollection.Add(new FdDescs()
                    {
                        Recordkey = record.Code,
                        FdDescription = record.Description,
                        RecordGuid = record.Guid,
                        FdExplanation = "FD"
                    }));
                allAstrsValues.ToList().ForEach(record =>
                    unCollection.Add(new UnDescs()
                    {
                        Recordkey = record.Code,
                        UnDescription = record.Description,
                        RecordGuid = record.Guid,
                        UnExplanation = "UN"
                    }));
                allAstrsValues.ToList().ForEach(record =>
                    obCollection.Add(new ObDescs()
                    {
                        Recordkey = record.Code,
                        ObDescription = record.Description,
                        RecordGuid = record.Guid,
                        ObExplanation = "OB"
                    }));
                allAstrsValues.ToList().ForEach(record =>
                    fcCollection.Add(new FcDescs()
                    {
                        Recordkey = record.Code,
                        FcDescription = record.Description,
                        RecordGuid = record.Guid,
                        FcExplanation = "FC"
                    }));
                allAstrsValues.ToList().ForEach(record =>
                    soCollection.Add(new SoDescs()
                    {
                        Recordkey = record.Code,
                        SoDescription = record.Description,
                        RecordGuid = record.Guid,
                        SoExplanation = "SO"
                    }));
                allAstrsValues.ToList().ForEach(record =>
                    loCollection.Add(new LoDescs()
                    {
                        Recordkey = record.Code,
                        LoDescription = record.Description,
                        RecordGuid = record.Guid,
                        LoExplanation = "LO"
                    }));

                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<FdDescs>("FD.DESCS", "", true))
                    .ReturnsAsync(fdCollection);
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<UnDescs>("UN.DESCS", "", true))
                    .ReturnsAsync(unCollection);
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<ObDescs>("OB.DESCS", "", true))
                    .ReturnsAsync(obCollection);
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<FcDescs>("FC.DESCS", "", true))
                     .ReturnsAsync(fcCollection);
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<SoDescs>("SO.DESCS", "", true))
                     .ReturnsAsync(soCollection);
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<LoDescs>("LO.DESCS", "", true))
                     .ReturnsAsync(loCollection);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));


                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var commodity = allAstrsValues.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "FD.DESCS", commodity.Code }),
                            new RecordKeyLookupResult() { Guid = commodity.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new ColleagueFinanceReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

                return referenceDataRepo;
            }
        }

        [TestClass]
        public class IntgVendorAddressUsagesTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<IntgVendorAddressUsages> allIntgVendorAddressUsages;
            ApplValcodes vendorAddressUsagesValcodeResponse;
            string domainEntityNameName;
            ApiSettings apiSettings;

            Mock<IColleagueFinanceReferenceDataRepository> referenceDataRepositoryMock;
            IColleagueFinanceReferenceDataRepository referenceDataRepository;
            ColleagueFinanceReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                allIntgVendorAddressUsages = new TestColleagueFinanceReferenceDataRepository().GetIntgVendorAddressUsagesAsync(false).Result;
                vendorAddressUsagesValcodeResponse = BuildValcodeResponse(allIntgVendorAddressUsages);
                var vendorAddressUsagesValResponse = new List<string>() { "2" };
                vendorAddressUsagesValcodeResponse.ValActionCode1 = vendorAddressUsagesValResponse;

                referenceDataRepositoryMock = new Mock<IColleagueFinanceReferenceDataRepository>();
                referenceDataRepository = referenceDataRepositoryMock.Object;

                referenceDataRepo = BuildValidReferenceDataRepository();
                domainEntityNameName = referenceDataRepo.BuildFullCacheKey("CF_INTG.VENDOR.ADDRESS.USAGES_GUID");

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                   x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                   .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                vendorAddressUsagesValcodeResponse = null;
                allIntgVendorAddressUsages = null;
                referenceDataRepo = null;
            }


            [TestMethod]
            public async Task ColleagueFinanceReferenceDataRepo_GetsIntgVendorAddressUsagesCacheAsync()
            {
                var vendorAddressUsages = await referenceDataRepo.GetIntgVendorAddressUsagesAsync(false);

                for (int i = 0; i < allIntgVendorAddressUsages.Count(); i++)
                {
                    Assert.AreEqual(allIntgVendorAddressUsages.ElementAt(i).Code, vendorAddressUsages.ElementAt(i).Code);
                    Assert.AreEqual(allIntgVendorAddressUsages.ElementAt(i).Description, vendorAddressUsages.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task ColleagueFinanceReferenceDataRepo_GetsIntgVendorAddressUsagesNonCacheAsync()
            {
                var statuses = await referenceDataRepo.GetIntgVendorAddressUsagesAsync(true);

                for (int i = 0; i < allIntgVendorAddressUsages.Count(); i++)
                {
                    Assert.AreEqual(allIntgVendorAddressUsages.ElementAt(i).Code, statuses.ElementAt(i).Code);
                    Assert.AreEqual(allIntgVendorAddressUsages.ElementAt(i).Description, statuses.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task ColleagueFinanceReferenceDataRepo_GetIntgVendorAddressUsages_WritesToCacheAsync()
            {

                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                cacheProviderMock.Setup(x => x.Contains(domainEntityNameName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(domainEntityNameName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CF.VALCODES", "INTG.VENDOR.ADDRESS.USAGES", It.IsAny<bool>())).ReturnsAsync(vendorAddressUsagesValcodeResponse);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // But after data accessor read, set up mocking so we can verify the list of vendorAddressUsages was written to the cache
                cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<List<IntgVendorAddressUsages>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                cacheProviderMock.Setup(x => x.Contains(referenceDataRepo.BuildFullCacheKey("CF_INTG.VENDOR.ADDRESS.USAGES"), null)).Returns(true);
                var vendorAddressUsages = await referenceDataRepo.GetIntgVendorAddressUsagesAsync(false);
                cacheProviderMock.Setup(x => x.Get(referenceDataRepo.BuildFullCacheKey("CF_INTG.VENDOR.ADDRESS.USAGES"), null)).Returns(vendorAddressUsages);
                // Verify that vendorAddressUsages were returned, which means they came from the "repository".
                Assert.IsTrue(vendorAddressUsages.Count() == 1);

                // Verify that the vendorAddressUsages item was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<List<IntgVendorAddressUsages>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);

            }

            [TestMethod]
            public async Task ColleagueFinanceReferenceDataRepo_GetIntgVendorAddressUsages_GetsCachedIntgVendorAddressUsagesAsync()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item (in this case the "INTG.VENDOR.ADDRESS.USAGES" cache item)
                cacheProviderMock.Setup(x => x.Contains(domainEntityNameName, null)).Returns(true);
                cacheProviderMock.Setup(x => x.Get(domainEntityNameName, null)).Returns(allIntgVendorAddressUsages).Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CF.VALCODES", "INTG.VENDOR.ADDRESS.USAGES", true)).ReturnsAsync(new ApplValcodes());

                // Assert the vendorAddressUsages are returned
                Assert.IsTrue((await referenceDataRepo.GetIntgVendorAddressUsagesAsync(false)).Count() == 1);
                // Verify that the svendorAddressUsages were retrieved from cache
                cacheProviderMock.Verify(m => m.Get(domainEntityNameName, null));
            }

            private ColleagueFinanceReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to vendorAddressUsages domainEntityName read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CF.VALCODES", "INTG.VENDOR.ADDRESS.USAGES", It.IsAny<bool>())).ReturnsAsync(vendorAddressUsagesValcodeResponse);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var vendorAddressUsages = allIntgVendorAddressUsages.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "CF.VALCODES", "INTG.VENDOR.ADDRESS.USAGES", vendorAddressUsages.Code }),
                            new RecordKeyLookupResult() { Guid = vendorAddressUsages.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new ColleagueFinanceReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<IntgVendorAddressUsages> vendorAddressUsages)
            {
                ApplValcodes vendorAddressUsagesResponse = new ApplValcodes();
                vendorAddressUsagesResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in vendorAddressUsages)
                {
                    vendorAddressUsagesResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "2", item.Code, "3", "", ""));
                }
                return vendorAddressUsagesResponse;
            }
        }
    }
}